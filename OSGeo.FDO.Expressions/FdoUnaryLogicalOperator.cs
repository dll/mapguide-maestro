﻿#region Disclaimer / License

// Copyright (C) 2015, Jackie Ng
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
using Irony.Parsing;

namespace OSGeo.FDO.Expressions
{
    /// <summary>
    /// An FDO unary logical operator
    /// </summary>
    public class FdoUnaryLogicalOperator : FdoLogicalOperator
    {
        /// <summary>
        /// The filter type
        /// </summary>
        public override FilterType FilterType => FilterType.UnaryLogicalOperator;

        /// <summary>
        /// The negated filter
        /// </summary>
        public FdoFilter NegatedFilter { get; }

        internal FdoUnaryLogicalOperator(ParseTreeNode node)
        {
            this.NegatedFilter = FdoFilter.ParseNode(node.ChildNodes[1]);
        }
    }
}