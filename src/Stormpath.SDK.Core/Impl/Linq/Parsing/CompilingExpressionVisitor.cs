﻿// <copyright file="CompilingExpressionVisitor.cs" company="Stormpath, Inc.">
// Copyright (c) 2016 Stormpath, Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using System.Linq.Expressions;
using Stormpath.SDK.Impl.Linq.Parsing.Expressions;
using Stormpath.SDK.Impl.Linq.Parsing.Expressions.ResultOperators;
using Stormpath.SDK.Impl.Linq.Parsing.NameTranslators;
using Stormpath.SDK.Impl.Linq.QueryModel;

namespace Stormpath.SDK.Impl.Linq.Parsing
{
    internal sealed class CompilingExpressionVisitor : ExpressionVisitor
    {
        private static readonly int DefaultApiPageLimit = 100;

        private FieldNameTranslator fieldNameTranslator
            = new FieldNameTranslator();

        private DateTimeFieldNameTranslator datetimeFieldNameTranslator
            = new DateTimeFieldNameTranslator();

        private MethodNameTranslator methodNameTranslator
            = new MethodNameTranslator();

        public CollectionResourceQueryModel Model { get; private set; }
            = new CollectionResourceQueryModel();

        internal Expression VisitParsedExpression(ParsedExpression node)
        {
            throw new NotSupportedException($"{node.GetType()} is an unsupported expression.");
        }

        internal Expression VisitTake(TakeExpression node)
        {
            if (this.Model.Limit.HasValue)
            {
                return node; // LIFO behavior
            }

            var value = node.Value;

            if (value == 0)
            {
                return node; // Take(0) is idempotent
            }

            this.Model.ExecutionPlan.MaxItems = value;
            this.Model.Limit = value;

            if (value > DefaultApiPageLimit)
            {
                this.Model.Limit = DefaultApiPageLimit;
            }

            return node;
        }

        internal Expression VisitSkip(SkipExpression node)
        {
            if (this.Model.Offset.HasValue)
            {
                return node; // LIFO behavior
            }

            this.Model.Offset = node.Value;

            return node;
        }

        internal Expression VisitOrderBy(OrderByExpression node)
        {
            string translatedFieldName = null;

            // Custom Data access doesn't need to be translated - use verbatim
            if (node.FieldName.StartsWith("customData.", StringComparison.Ordinal))
            {
                translatedFieldName = node.FieldName;
            }
            // Other field names do (mostly for correct camelCasing; see the translator classes)
            else if (!this.fieldNameTranslator.TryGetValue(node.FieldName, out translatedFieldName))
            {
                throw new NotSupportedException($"{node.FieldName} is not a supported field.");
            }

            this.Model.OrderByTerms.Add(new OrderByTerm()
            {
                FieldName = translatedFieldName,
                Direction = node.Direction
            });

            return node;
        }

        internal Expression VisitFilter(FilterExpression node)
        {
            if (!string.IsNullOrEmpty(this.Model.FilterTerm))
            {
                throw new NotSupportedException("Multiple Filter terms are not supported");
            }

            this.Model.FilterTerm = node.Value;

            return node;
        }

        internal Expression VisitExpand(ExpandExpression node)
        {
            string translatedMethodName = null;
            if (!this.methodNameTranslator.TryGetValue(node.MethodName, out translatedMethodName))
            {
                throw new NotSupportedException($"{node.MethodName} does not support expansion.");
            }

            this.Model.ExpandTerms.Add(new ExpandTerm()
            {
                PropertyName = translatedMethodName, Offset = node.Offset, Limit = node.Limit
            });

            return node;
        }

        internal Expression VisitWhereMember(WhereMemberExpression node)
        {
            string translatedFieldName = null;

            // Custom Data access doesn't need to be translated - use verbatim
            if (node.FieldName.StartsWith("customData.", StringComparison.Ordinal))
            {
                translatedFieldName = node.FieldName;
            }
            // Other field names do (mostly for correct camelCasing; see the translator classes)
            else if (!this.fieldNameTranslator.TryGetValue(node.FieldName, out translatedFieldName) &&
                !this.datetimeFieldNameTranslator.TryGetValue(node.FieldName, out translatedFieldName))
            {
                throw new NotSupportedException($"{node.FieldName} is not a supported field.");
            }

            this.Model.WhereTerms.Add(new WhereTerm()
            {
                FieldName = translatedFieldName,
                Comparison = node.Comparison,
                Value = node.Value,
                Type = node.Value.GetType()
            });

            return node;
        }

        internal Expression VisitResultOperator(ResultOperatorExpression node)
        {
            this.Model.ResultOperator = node.ResultType;

            if (node.ResultType == ResultOperator.First)
            {
                this.Model.ResultDefaultIfEmpty = (node as FirstResultOperator).DefaultIfEmpty;
            }
            else if (node.ResultType == ResultOperator.Single)
            {
                this.Model.ResultDefaultIfEmpty = (node as SingleResultOperator).DefaultIfEmpty;
            }

            return node;
        }
    }
}
