﻿#region Disclaimer / License

// Copyright (C) 2013, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
//

#endregion Disclaimer / License

using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using ICSharpCode.TextEditor.Gui.CompletionWindow;
using OSGeo.MapGuide.MaestroAPI;
using OSGeo.MapGuide.MaestroAPI.Schema;
using OSGeo.MapGuide.ObjectModels.Capabilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Maestro.Editors.Common.Expression
{
    //NOTE:
    //
    //It seems the auto-complete capabilities of the ICSharpCode.TextEditor assume object-oriented languages or languages
    //that involve a member access operator (., -> or anything similar), so we have to use a custom ICompletionData that
    //compensates for the lack of such contexts
    //
    //NOTE/TODO:
    //Auto-completions are currently case-sensitive and will only trigger on the correct case.

    internal class FdoExpressionCompletionDataProvider : ICompletionDataProvider, IDisposable
    {
        private ClassDefinition _klass;
        private IFdoProviderCapabilities _caps;

        public FdoExpressionCompletionDataProvider(ClassDefinition cls, IFdoProviderCapabilities caps)
        {
            _klass = cls;
            _caps = caps;
            this.DefaultIndex = 0;
            this.PreSelection = null;
            this.ImageList = new System.Windows.Forms.ImageList();
            this.ImageList.Images.Add(Properties.Resources.block);
            this.ImageList.Images.Add(Properties.Resources.property);
            this.ImageList.Images.Add(Properties.Resources.funnel);
        }

        ~FdoExpressionCompletionDataProvider()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.ImageList?.Dispose();
                this.ImageList = null;
            }
        }

        public System.Windows.Forms.ImageList ImageList { get; private set; }

        public string PreSelection { get; }

        public int DefaultIndex { get; }

        public bool InsertSpace
        {
            get;
            set;
        }

        public CompletionDataProviderKeyResult ProcessKey(char key)
        {
            CompletionDataProviderKeyResult res;
            if (key == ' ' && this.InsertSpace)
            {
                this.InsertSpace = false; // insert space only once
                res = CompletionDataProviderKeyResult.BeforeStartKey;
            }
            else if (char.IsLetterOrDigit(key) || key == '_')
            {
                this.InsertSpace = false; // don't insert space if user types normally
                res = CompletionDataProviderKeyResult.NormalKey;
            }
            else
            {
                // do not reset insertSpace when doing an insertion!
                res = CompletionDataProviderKeyResult.InsertionKey;
            }
            return res;
        }

        public bool InsertAction(ICompletionData data, TextArea textArea, int insertionOffset, char key)
        {
            if (this.InsertSpace)
            {
                textArea.Document.Insert(insertionOffset++, " ");
            }
            textArea.Caret.Position = textArea.Document.OffsetToPosition(insertionOffset);

            var res = data.InsertAction(textArea, key);
            var fdoComp = (FdoCompletionData)data;
            if (fdoComp.ImageIndex == 0 && fdoComp.AppendText.Length > 2) //Function and not an empty function call
            {
                //Rewind caret so it is at the start of the function call (at first parameter)
                var offset = textArea.Caret.Offset;
                offset -= (fdoComp.AppendText.Length - 1);
                textArea.Caret.Position = textArea.Document.OffsetToPosition(offset);
                textArea.SelectionManager.ClearSelection();
                textArea.SelectionManager.SetSelection(textArea.Caret.Position, textArea.Document.OffsetToPosition(offset + (fdoComp.HighlightLength - 1)));
            }
            return res;
        }

        /// <summary>
        /// Gets the line of text up to the cursor position.
        /// </summary>
        private string GetLineText(TextArea textArea)
        {
            LineSegment lineSegment = textArea.Document.GetLineSegmentForOffset(textArea.Caret.Offset);
            return textArea.Document.GetText(lineSegment);
        }

        public ICompletionData[] GenerateCompletionData(string fileName, TextArea textArea, char charTyped)
            => GenerateCompletionData((GetLineText(textArea) + charTyped).Trim());

        private class FdoCompletionData : DefaultCompletionData
        {
            private readonly string _insertText;
            private readonly string _appendText;
            private int _highlightLength = 0;

            public int HighlightLength => _highlightLength;

            public string InsertText => _insertText;

            public string AppendText => _appendText;

            public FdoCompletionData(string prefix, string text, string description, int imageIndex)
                : base(text, description, imageIndex)
            {
                _insertText = text.Substring(prefix.Length);
                _appendText = string.Empty;
            }

            public FdoCompletionData(string prefix, string text, string description, string appendText, int highlightLength, int imageIndex)
                : this(prefix, text, description, imageIndex)
            {
                _appendText = appendText;
                _highlightLength = highlightLength;
            }

            public override bool InsertAction(TextArea textArea, char ch)
            {
                textArea.InsertString(_insertText + _appendText);
                return false;
            }
        }

        private ICompletionData[] GenerateCompletionData(string line)
        {
            Debug.WriteLine("FDO auto-complete: " + line);
            List<DefaultCompletionData> items = new List<DefaultCompletionData>();
            string name = GetName(line);
            if (!String.IsNullOrEmpty(name))
            {
                try
                {
                    foreach (var func in GetMatchingFdoFunctions(name))
                    {
                        foreach (var sign in func.Signatures)
                        {
                            var member = CreateFdoFunctionSignatureDescriptor(func, sign);
                            int highlightLength = 0;
                            var args = sign.Arguments;
                            if (args.Length > 0)
                            {
                                highlightLength = args[0].Name.Length + 2; // [ and ]
                            }
                            items.Add(new FdoCompletionData(name, member.Name, member.Description, member.AppendText, highlightLength, 0));
                        }
                    }
                    foreach (var member in GetMatchingClassProperties(name))
                    {
                        items.Add(new FdoCompletionData(name, member.Name, member.Description, 1));
                    }
                    foreach (var member in GetMatchingFdoConditions(name))
                    {
                        if (string.IsNullOrEmpty(member.AppendText))
                            items.Add(new FdoCompletionData(name, member.Name, member.Description, 2));
                        else
                            items.Add(new FdoCompletionData(name, member.Name, member.Description, member.AppendText, member.AppendText.Length - 1, 2));
                    }
                    foreach (var member in GetMatchingFdoOperators(name))
                    {
                        if (string.IsNullOrEmpty(member.AppendText))
                            items.Add(new FdoCompletionData(name, member.Name, member.Description, 2));
                        else
                            items.Add(new FdoCompletionData(name, member.Name, member.Description, member.AppendText, 0, 2));
                    }
                    items.Sort((a, b) => { return a.Text.CompareTo(b.Text); });
                }
                catch
                {
                    // Do nothing.
                }
            }
            return items.ToArray();
        }

        private class Descriptor
        {
            public string Name;
            public string Description;
            public string AppendText;
        }

        private IEnumerable<IFdoFunctionDefintion> GetMatchingFdoFunctions(string name)
        {
            foreach (var func in _caps.Expression.SupportedFunctions.Concat(Utility.GetStylizationFunctions()))
            {
                if (func.Name.StartsWith(name))
                    yield return func;
            }
        }

        private IEnumerable<Descriptor> GetMatchingFdoConditions(string name)
        {
            foreach (var cond in _caps.Filter.ConditionTypes)
            {
                if (cond.ToString().ToUpper().StartsWith(name))
                {
                    var desc = CreateFdoConditionDescriptor(cond);
                    if (desc != null)
                        yield return desc;
                }
            }
        }

        private Descriptor CreateFdoConditionDescriptor(string cond)
        {
            switch (cond)
            {
                case "Null":
                    return new Descriptor()
                    {
                        Name = cond.ToString().ToUpper(),
                        Description = "[property] NULL" //NOXLATE
                    };
                case "In":
                    return new Descriptor()
                    {
                        Name = cond.ToString().ToUpper(),
                        Description = "[property] IN ([value1], [value2], ..., [valueN])", //NOXLATE
                        AppendText = " ([value1], [value2])" //NOXLATE
                    };
                case "Like":
                    return new Descriptor()
                    {
                        Name = cond.ToString().ToUpper(),
                        Description = "[property] LIKE [string value]", //NOXLATE
                        AppendText = " [string value]" //NOXLATE
                    };
            }

            return null; //Handled by operators
        }

        private static Descriptor CreateBinaryDistanceOperator(string opName)
        {
            return new Descriptor()
            {
                Name = opName.ToUpper(),
                Description = $"[property] {opName} [number]", //NOXLATE
                AppendText = " [number]" //NOXLATE
            };
        }

        private static Descriptor CreateBinarySpatialOperator(string opName)
        {
            return new Descriptor()
            {
                Name = opName.ToUpper(),
                Description = $"[geometry] {opName} GeomFromText('geometry wkt')", //NOXLATE
                AppendText = " GeomFromText('geometry wkt')" //NOXLATE
            };
        }

        private IEnumerable<Descriptor> GetMatchingFdoOperators(string name)
        {
            foreach (var op in _caps.Filter.DistanceOperations)
            {
                var opName = op.ToUpper();
                if (opName.StartsWith(name))
                    yield return CreateBinaryDistanceOperator(opName);
            }
            foreach (var op in _caps.Filter.SpatialOperations)
            {
                var opName = op.ToUpper();
                if (opName.StartsWith(name))
                    yield return CreateBinarySpatialOperator(opName);
            }
        }

        private IEnumerable<Descriptor> GetMatchingClassProperties(string name)
        {
            foreach (var prop in _klass.Properties)
            {
                if (prop.Name.StartsWith(name))
                    yield return CreatePropertyDescriptor(prop);
            }
        }

        private Descriptor CreateFdoFunctionSignatureDescriptor(IFdoFunctionDefintion func, IFdoFunctionDefintionSignature sig)
        {
            var desc = new Descriptor();
            List<string> args = new List<string>();
            foreach (var argDef in sig.Arguments)
            {
                args.Add(argDef.Name.Trim());
            }
            string argsStr = StringifyFunctionArgs(args);
            string argDesc = DescribeSignature(sig);
            string expr = $"{func.Name}({argsStr})"; //NOXLAT
            desc.Name = expr;
            desc.Description = string.Format(Strings.ExprEditorFunctionDesc, expr, func.Description, argDesc, sig.ReturnType, Environment.NewLine);
            desc.AppendText = string.Empty;
            return desc;
        }

        internal static string DescribeSignature(IFdoFunctionDefintionSignature sig)
        {
            string argDesc = Strings.None;
            var args = sig.Arguments;
            if (args.Length > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(Environment.NewLine);
                foreach (var argDef in sig.Arguments)
                {
                    sb.Append($"  [{argDef.Name}] - {argDef.Description}{Environment.NewLine}"); //NOXLATE
                }
                argDesc = sb.ToString();
            }
            return argDesc;
        }

        internal static string StringifyFunctionArgs(List<string> args)
        {
            string argsStr = args.Count > 0 ? "[" + string.Join("], [", args.ToArray()) + "]" : string.Empty; //NOXLATE
            return argsStr;
        }

        private static Descriptor CreatePropertyDescriptor(PropertyDefinition prop)
        {
            var desc = new Descriptor();
            desc.Name = prop.Name;
            switch (prop.Type)
            {
                case PropertyDefinitionType.Geometry:
                    { 
                        var g = (GeometricPropertyDefinition)prop;
                        desc.Description = string.Format(Strings.FsPreview_GeometryPropertyNodeTooltip,
                            g.Name,
                            g.Description,
                            g.GeometryTypesToString(),
                            g.IsReadOnly,
                            g.HasElevation,
                            g.HasMeasure,
                            g.SpatialContextAssociation,
                            Environment.NewLine);
                    }
                    break;
                case PropertyDefinitionType.Data:
                    {
                        var d = (DataPropertyDefinition)prop;
                        desc.Description = string.Format(Strings.FsPreview_DataPropertyNodeTooltip,
                            d.Name,
                            d.Description,
                            d.DataType.ToString(),
                            d.IsNullable,
                            d.IsReadOnly,
                            d.Length,
                            d.Precision,
                            d.Scale,
                            Environment.NewLine);
                    }
                    break;
                case PropertyDefinitionType.Raster:
                    {
                        var r = (RasterPropertyDefinition)prop;
                        desc.Description = string.Format(Strings.FsPreview_RasterPropertyNodeTooltip,
                            r.Name,
                            r.Description,
                            r.IsNullable,
                            r.DefaultImageXSize,
                            r.DefaultImageYSize,
                            r.SpatialContextAssociation,
                            Environment.NewLine);
                    }
                    break;
                default:
                    {
                        desc.Description = string.Format(Strings.FsPreview_GenericPropertyTooltip,
                            prop.Name,
                            prop.Type.ToString(),
                            Environment.NewLine);
                    }
                    break;
            }

            return desc;
        }

        private string GetName(string text)
        {
            int startIndex = text.LastIndexOfAny(new char[] { ' ', '+', '/', '*', '-', '%', '=', '>', '<', '&', '|', '^', '~', '(', ',', ')' }); //NOXLATE
            string res = text.Substring(startIndex + 1);
            Debug.WriteLine("Evaluating FDO auto-complete options for: " + res);
            return res;
        }
    }
}