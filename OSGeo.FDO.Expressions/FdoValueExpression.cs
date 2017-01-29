﻿#region Disclaimer / License

// Copyright (C) 2015, Jackie Ng
// http://trac.osgeo.org/mapguide/wiki/maestro, jumpinjackie@gmail.com
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
using Irony.Parsing;

namespace OSGeo.FDO.Expressions
{
    /// <summary>
    /// The base class of all value expressions
    /// </summary>
    public abstract class FdoValueExpression : FdoExpression
    {
        internal static FdoValueExpression ParseValueNode(ParseTreeNode node)
        {
            if (node.Term.Name == FdoTerminalNames.ValueExpression)
            {
                return ParseValueNode(node.ChildNodes[0]);
            }
            else
            {
                switch (node.Term.Name)
                {
                    case FdoTerminalNames.LiteralValue:
                        return FdoLiteralValue.ParseLiteralNode(node);
                    case FdoTerminalNames.Parameter:
                        return new FdoParameter(node);
                    default:
                        throw new FdoParseException("Unknown terminal: " + node.Term.Name);
                }
            }
        }
    }
}
