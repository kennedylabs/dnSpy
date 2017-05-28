﻿/*
    Copyright (C) 2014-2017 de4dot@gmail.com

    This file is part of dnSpy

    dnSpy is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    dnSpy is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with dnSpy.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Threading;
using dnSpy.Contracts.Text;

namespace dnSpy.Contracts.Debugger.Evaluation {
	/// <summary>
	/// A value shown in a treeview (eg. in locals window)
	/// </summary>
	public abstract class DbgValueNode : DbgObject {
		/// <summary>
		/// Gets the language
		/// </summary>
		public abstract DbgLanguage Language { get; }

		/// <summary>
		/// Gets the process
		/// </summary>
		public DbgProcess Process => Runtime.Process;

		/// <summary>
		/// Gets the runtime
		/// </summary>
		public abstract DbgRuntime Runtime { get; }

		/// <summary>
		/// true if this is an error value node, it has an error message and not a value
		/// </summary>
		public bool HasError => ErrorMessage != null;

		/// <summary>
		/// Gets the error message or null
		/// </summary>
		public abstract string ErrorMessage { get; }

		/// <summary>
		/// true if <see cref="Value"/> is not null
		/// </summary>
		public bool HasValue => Value != null;

		/// <summary>
		/// Gets the value or null if there was an error (see <see cref="ErrorMessage"/>)
		/// </summary>
		public abstract DbgValue Value { get; }

		/// <summary>
		/// Gets the expression that is used when adding an expression to the watch window or
		/// when assigning a new value to the source.
		/// </summary>
		public abstract string Expression { get; }

		/// <summary>
		/// Image name, see <see cref="PredefinedDbgValueNodeImageNames"/>
		/// </summary>
		public abstract string ImageName { get; }

		/// <summary>
		/// true if this is a read-only value
		/// </summary>
		public abstract bool IsReadOnly { get; }

		/// <summary>
		/// true if the expression causes side effects
		/// </summary>
		public abstract bool CausesSideEffects { get; }

		/// <summary>
		/// Returns true if it has children, false if it has no children and null if it's unknown (eg. it's too expensive to calculate it now).
		/// UI code can use this property to decide if it shows the treeview node expander ("|>").
		/// </summary>
		public abstract bool? HasChildren { get; }

		/// <summary>
		/// Number of children. This property is called as late as possible and can be lazily initialized.
		/// It's assumed to be 0 if <see cref="HasChildren"/> is false.
		/// </summary>
		public abstract ulong ChildCount { get; }

		/// <summary>
		/// Creates new children. This method blocks the current thread until the children have been created.
		/// The returned <see cref="DbgValueNode"/>s are automatically closed when their runtime continues
		/// </summary>
		/// <param name="context">Evaluation context</param>
		/// <param name="index">Index of first child</param>
		/// <param name="count">Max number of children to return</param>
		/// <param name="options">Options</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns></returns>
		public abstract DbgValueNode[] GetChildren(DbgEvaluationContext context, ulong index, int count, DbgValueNodeEvaluationOptions options, CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>
		/// Creates new children. The returned <see cref="DbgValueNode"/>s are automatically closed when their runtime continues
		/// </summary>
		/// <param name="context">Evaluation context</param>
		/// <param name="index">Index of first child</param>
		/// <param name="count">Max number of children to return</param>
		/// <param name="options">Options</param>
		/// <param name="callback">Called when this method is complete</param>
		/// <param name="cancellationToken">Cancellation token</param>
		public abstract void GetChildren(DbgEvaluationContext context, ulong index, int count, DbgValueNodeEvaluationOptions options, Action<DbgValueNode[]> callback, CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>
		/// Formats the name. This method blocks the current thread until all requested values have been formatted
		/// </summary>
		/// <param name="context">Evaluation context</param>
		/// <param name="output">Output</param>
		/// <param name="cancellationToken">Cancellation token</param>
		public void FormatName(DbgEvaluationContext context, ITextColorWriter output, CancellationToken cancellationToken = default(CancellationToken)) =>
			Format(context, new DbgValueNodeFormatParameters { NameOutput = output ?? throw new ArgumentNullException(nameof(output)) }, cancellationToken);

		/// <summary>
		/// Formats the value. This method blocks the current thread until all requested values have been formatted
		/// </summary>
		/// <param name="context">Evaluation context</param>
		/// <param name="output">Output</param>
		/// <param name="options">Formatter options</param>
		/// <param name="cancellationToken">Cancellation token</param>
		public void FormatValue(DbgEvaluationContext context, ITextColorWriter output, DbgValueFormatterOptions options, CancellationToken cancellationToken = default(CancellationToken)) =>
			Format(context, new DbgValueNodeFormatParameters {
				ValueOutput = output ?? throw new ArgumentNullException(nameof(output)),
				ValueFormatterOptions = options,
			}, cancellationToken);

		/// <summary>
		/// Formats the expected type ("field" type). This method blocks the current thread until all requested values have been formatted
		/// </summary>
		/// <param name="context">Evaluation context</param>
		/// <param name="output">Output</param>
		/// <param name="options">Formatter options</param>
		/// <param name="cancellationToken">Cancellation token</param>
		public void FormatExpectedType(DbgEvaluationContext context, ITextColorWriter output, DbgValueFormatterTypeOptions options, CancellationToken cancellationToken = default(CancellationToken)) =>
			Format(context, new DbgValueNodeFormatParameters {
				ExpectedTypeOutput = output ?? throw new ArgumentNullException(nameof(output)),
				ExpectedTypeFormatterOptions = options,
			}, cancellationToken);

		/// <summary>
		/// Formats the actual type (value type). This method blocks the current thread until all requested values have been formatted
		/// </summary>
		/// <param name="context">Evaluation context</param>
		/// <param name="output">Output</param>
		/// <param name="options">Formatter options</param>
		/// <param name="cancellationToken">Cancellation token</param>
		public void FormatActualType(DbgEvaluationContext context, ITextColorWriter output, DbgValueFormatterTypeOptions options, CancellationToken cancellationToken = default(CancellationToken)) =>
			Format(context, new DbgValueNodeFormatParameters {
				ActualTypeOutput = output ?? throw new ArgumentNullException(nameof(output)),
				ActualTypeFormatterOptions = options,
			}, cancellationToken);

		/// <summary>
		/// Formats the name, value, and type. This method blocks the current thread until all requested values have been formatted
		/// </summary>
		/// <param name="context">Evaluation context</param>
		/// <param name="options">Options</param>
		/// <param name="cancellationToken">Cancellation token</param>
		public abstract void Format(DbgEvaluationContext context, IDbgValueNodeFormatParameters options, CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>
		/// Formats the name, value, and type
		/// </summary>
		/// <param name="context">Evaluation context</param>
		/// <param name="options">Options</param>
		/// <param name="callback">Called when this method is complete</param>
		/// <param name="cancellationToken">Cancellation token</param>
		public abstract void Format(DbgEvaluationContext context, IDbgValueNodeFormatParameters options, Action callback, CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>
		/// Writes a new value. It blocks the current thread until the assignment is complete.
		/// </summary>
		/// <param name="context">Evaluation context</param>
		/// <param name="expression">Source expression (rhs)</param>
		/// <param name="options">Options</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns></returns>
		public abstract DbgValueNodeAssignmentResult Assign(DbgEvaluationContext context, string expression, DbgEvaluationOptions options, CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>
		/// Writes a new value
		/// </summary>
		/// <param name="context">Evaluation context</param>
		/// <param name="expression">Source expression (rhs)</param>
		/// <param name="options">Options</param>
		/// <param name="callback">Called when this method is complete</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns></returns>
		public abstract void Assign(DbgEvaluationContext context, string expression, DbgEvaluationOptions options, Action<DbgValueNodeAssignmentResult> callback, CancellationToken cancellationToken = default(CancellationToken));
	}

	/// <summary>
	/// Assignment result
	/// </summary>
	public struct DbgValueNodeAssignmentResult {
		/// <summary>
		/// Gets the error message or null if none
		/// </summary>
		public string Error { get; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="error">Error message or null if none</param>
		public DbgValueNodeAssignmentResult(string error) => Error = error;
	}

	/// <summary>
	/// Parameters when formatting a <see cref="DbgValueNode"/>
	/// </summary>
	public interface IDbgValueNodeFormatParameters {
		/// <summary>
		/// Used when writing the name or null if it's not written
		/// </summary>
		ITextColorWriter NameOutput { get; }

		/// <summary>
		/// Used when writing the value or null if it's not written
		/// </summary>
		ITextColorWriter ValueOutput { get; }

		/// <summary>
		/// Used when writing the expected type ("field" type) or null if it's not written
		/// </summary>
		ITextColorWriter ExpectedTypeOutput { get; }

		/// <summary>
		/// Used when writing the actual type (value type) or null if it's not written
		/// </summary>
		ITextColorWriter ActualTypeOutput { get; }

		/// <summary>
		/// Value formatter options
		/// </summary>
		DbgValueFormatterOptions ValueFormatterOptions { get; }

		/// <summary>
		/// Expected type formatter options
		/// </summary>
		DbgValueFormatterTypeOptions ExpectedTypeFormatterOptions { get; }

		/// <summary>
		/// Actual type formatter options
		/// </summary>
		DbgValueFormatterTypeOptions ActualTypeFormatterOptions { get; }
	}

	sealed class DbgValueNodeFormatParameters : IDbgValueNodeFormatParameters {
		public ITextColorWriter NameOutput { get; set; }
		public ITextColorWriter ValueOutput { get; set; }
		public ITextColorWriter ExpectedTypeOutput { get; set; }
		public ITextColorWriter ActualTypeOutput { get; set; }
		public DbgValueFormatterOptions ValueFormatterOptions { get; set; }
		public DbgValueFormatterTypeOptions ExpectedTypeFormatterOptions { get; set; }
		public DbgValueFormatterTypeOptions ActualTypeFormatterOptions { get; set; }
	}
}