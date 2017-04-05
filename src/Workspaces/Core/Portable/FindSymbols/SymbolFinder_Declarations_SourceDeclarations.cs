﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Internal.Log;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.FindSymbols
{
    // All the logic for finding source declarations in a given solution/project with some name 
    // is in this file.  

    public static partial class SymbolFinder
    {
        /// <summary>
        /// Find the symbols for declarations made in source with the specified name.
        /// </summary>
        public static Task<IEnumerable<ISymbol>> FindSourceDeclarationsAsync(Solution solution, string name, bool ignoreCase, CancellationToken cancellationToken = default(CancellationToken))
            => FindSourceDeclarationsAsync(solution, name, ignoreCase, SymbolFilter.All, cancellationToken);

        /// <summary>
        /// Find the symbols for declarations made in source with the specified name.
        /// </summary>
        public static async Task<IEnumerable<ISymbol>> FindSourceDeclarationsAsync(
            Solution solution, string name, bool ignoreCase, SymbolFilter filter, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (solution == null)
            {
                throw new ArgumentNullException(nameof(solution));
            }

            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                return ImmutableArray<ISymbol>.Empty;
            }

            using (Logger.LogBlock(FunctionId.SymbolFinder_Solution_Name_FindSourceDeclarationsAsync, cancellationToken))
            {
                return await FindSourceDeclarationsWithNormalQueryAsync(
                    solution, SearchQuery.Create(name, ignoreCase), filter, cancellationToken).ConfigureAwait(false);
            }
        }

        private static async Task<ImmutableArray<ISymbol>> FindSourceDeclarationsWithNormalQueryAsync(
            Solution solution, SearchQuery query, SymbolFilter filter, CancellationToken cancellationToken)
        {
            Debug.Assert(query.Kind != SearchKind.Custom);

            if (query.Name != null && string.IsNullOrWhiteSpace(query.Name))
            {
                return ImmutableArray<ISymbol>.Empty;
            }

            var result = ArrayBuilder<ISymbol>.GetInstance();
            foreach (var projectId in solution.ProjectIds)
            {
                var project = solution.GetProject(projectId);
                await AddCompilationDeclarationsWithNormalQueryAsync(project, query, filter, result, cancellationToken).ConfigureAwait(false);
            }

            return result.ToImmutableAndFree();
        }

        /// <summary>
        /// Find the symbols for declarations made in source with the specified name.
        /// </summary>
        public static Task<IEnumerable<ISymbol>> FindSourceDeclarationsAsync(Project project, string name, bool ignoreCase, CancellationToken cancellationToken = default(CancellationToken))
            => FindSourceDeclarationsAsync(project, name, ignoreCase, SymbolFilter.All, cancellationToken);

        /// <summary>
        /// Find the symbols for declarations made in source with the specified name.
        /// </summary>
        public static async Task<IEnumerable<ISymbol>> FindSourceDeclarationsAsync(
            Project project, string name, bool ignoreCase, SymbolFilter filter, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                return SpecializedCollections.EmptyEnumerable<ISymbol>();
            }

            using (Logger.LogBlock(FunctionId.SymbolFinder_Project_Name_FindSourceDeclarationsAsync, cancellationToken))
            {
                var list = ArrayBuilder<ISymbol>.GetInstance();
                await AddCompilationDeclarationsWithNormalQueryAsync(
                    project, SearchQuery.Create(name, ignoreCase), filter, list, cancellationToken).ConfigureAwait(false);
                return list.ToImmutableAndFree();
            }
        }
    }
}