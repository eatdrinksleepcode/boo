#region license
// Copyright (c) 2004, Rodrigo B. de Oliveira (rbo@acm.org)
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification,
// are permitted provided that the following conditions are met:
// 
//     * Redistributions of source code must retain the above copyright notice,
//     this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright notice,
//     this list of conditions and the following disclaimer in the documentation
//     and/or other materials provided with the distribution.
//     * Neither the name of Rodrigo B. de Oliveira nor the names of its
//     contributors may be used to endorse or promote products derived from this
//     software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
// THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion

// TODO/FIXME:
//GenerateArrayCreateExpression -- not sure what its doing with array params
//GenerateIterationStatement
//GenerateMethod -- needs some polish

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Reflection;
using System.Collections;

namespace Boo.CodeDom
{
	/// <summary>
	/// 
	/// </summary>
	public class BooCodeGenerator : CodeGenerator
	{
		string currentType = "";
		public BooCodeGenerator () {
		}

		protected override string NullToken {
			get {
				return "null";
			}
		}
		// FIXME: Any way to create typed array?
		protected override void GenerateArrayCreateExpression ( CodeArrayCreateExpression expression)
		{
			TextWriter output = Output;

			output.Write( "array " );

			CodeExpressionCollection initializers = expression.Initializers;
			CodeTypeReference createType = expression.CreateType;
			output.Write( '(' );
			
			
			if (initializers.Count > 0) {
				output.WriteLine ("(");
				++Indent;
				OutputExpressionList ( initializers, true );
				--Indent;
				output.Write (")");

			} else {
				CodeTypeReference arrayType = createType.ArrayElementType;
				while ( arrayType != null ) {
					createType = arrayType;
					arrayType = arrayType.ArrayElementType;
				}

				output.Write( '(' );

				CodeExpression size = expression.SizeExpression;
				if ( size != null )
					GenerateExpression( size );
				else
					output.Write( expression.Size );

				output.Write( ')' );

			}
			output.Write( ')' );
			output.WriteLine ();
			
		}
		
		// no base keyword in boo -- use super instead
		protected override void GenerateBaseReferenceExpression ( CodeBaseReferenceExpression expression)
		{
			TextWriter output = Output;
			output.Write("super");
			
		}
		
		protected override void GenerateCastExpression ( CodeCastExpression expression)
		{
			TextWriter output = Output;
			output.Write( "cast(" );
			OutputType( expression.TargetType );
			output.Write( ", " );
			GenerateExpression( expression.Expression );
			output.Write( ")" );
		}


		protected override void GenerateCompileUnitStart ( CodeCompileUnit compileUnit)
		{
			GenerateComment( new CodeComment( "------------------------------------------------------------------------------" ) );
			GenerateComment( new CodeComment( " <autogenerated>" ) );
			GenerateComment( new CodeComment( "	 This code was generated by a tool." ) );
			GenerateComment( new CodeComment( "	 Runtime Version: " +  System.Environment.Version ) );
			GenerateComment( new CodeComment( "" ) );
			GenerateComment( new CodeComment( "	 Changes to this file may cause incorrect behavior and will be lost if " ) );
			GenerateComment( new CodeComment( "	 the code is regenerated." ) );
			GenerateComment( new CodeComment( " </autogenerated>" ) );
			GenerateComment( new CodeComment( "------------------------------------------------------------------------------" ) );
			Output.WriteLine();
		}

		protected override void GenerateDelegateCreateExpression (
			CodeDelegateCreateExpression expression)
		{
			TextWriter output = Output;
			output.Write( "cast(" );
			OutputType( expression.DelegateType );
	
			output.Write( ", " );
			
			CodeExpression targetObject = expression.TargetObject;
			if ( targetObject != null ) {
				GenerateExpression( targetObject );
				Output.Write( '.' );
			}
			output.Write( GetSafeName (expression.MethodName) );

			output.Write( ")" );
		}

		protected override void GenerateFieldReferenceExpression (
			CodeFieldReferenceExpression expression)
		{
			CodeExpression targetObject = expression.TargetObject;
			if (targetObject != null) {
				GenerateExpression (targetObject);
				Output.Write ('.');
			}
			Output.Write (GetSafeName (expression.FieldName));
		}
		
		protected override void GenerateArgumentReferenceExpression (
			CodeArgumentReferenceExpression expression)
		{
			Output.Write (GetSafeName (expression.ParameterName));
		}

		protected override void GenerateVariableReferenceExpression (
			CodeVariableReferenceExpression expression)
		{
			Output.Write (GetSafeName (expression.VariableName));
		}
			
		protected override void GenerateIndexerExpression (
			CodeIndexerExpression expression)
		{
			TextWriter output = Output;

			GenerateExpression (expression.TargetObject);
			output.Write ( '[' );
			OutputExpressionList( expression.Indices );
			output.Write( ']' );
		}
		
		protected override void GenerateArrayIndexerExpression (
			CodeArrayIndexerExpression expression)
		{
			TextWriter output = Output;

			GenerateExpression (expression.TargetObject);   
			output.Write ('[');
			OutputExpressionList (expression.Indices);
			output.Write (']');
		}
		
		protected override void GenerateSnippetExpression (
			CodeSnippetExpression expression)
		{
			Output.Write (expression.Value);
		}
		
		protected override void GenerateMethodInvokeExpression (
			CodeMethodInvokeExpression expression)
		{
			TextWriter output = Output;
    
			GenerateMethodReferenceExpression (expression.Method);

			output.Write ('(');
			OutputExpressionList (expression.Parameters);
			output.Write (')');
		}

		protected override void GenerateMethodReferenceExpression (
			CodeMethodReferenceExpression expression )
		{			
			if (expression.TargetObject != null)
			{
				GenerateExpression (expression.TargetObject);
				Output.Write ('.');
			}
			Output.Write (GetSafeName (expression.MethodName));
			
		}

		protected override void GenerateEventReferenceExpression (
			CodeEventReferenceExpression expression )
		{
			GenerateExpression (expression.TargetObject);
			Output.Write ('.');
			Output.Write (GetSafeName (expression.EventName));
		}

		protected override void GenerateDelegateInvokeExpression (
			CodeDelegateInvokeExpression expression )
		{
			GenerateExpression (expression.TargetObject);
			Output.Write ('(');
			OutputExpressionList (expression.Parameters);
			Output.Write (')');
		}
		
		protected override void GenerateObjectCreateExpression (
			CodeObjectCreateExpression expression )
		{
			OutputType (expression.CreateType);
			Output.Write ('(');
			OutputExpressionList (expression.Parameters);
			Output.Write (')');
		}

		protected override void GeneratePropertyReferenceExpression (
			CodePropertyReferenceExpression expression )
		{
			CodeExpression targetObject = expression.TargetObject;
			if (targetObject != null) {
				GenerateExpression (targetObject);
				Output.Write ('.');
			}
			Output.Write (GetSafeName (expression.PropertyName));
		}

		// FIXME: It is still C#-ism
		protected override void GeneratePropertySetValueReferenceExpression (
			CodePropertySetValueReferenceExpression expression)
		{
			Output.Write ( "value" );
		}

		protected override void GenerateThisReferenceExpression (
			CodeThisReferenceExpression expression)
		{
			Output.Write ("self");
		}
	
		protected override void GenerateExpressionStatement (
			CodeExpressionStatement statement) {
			GenerateExpression(statement.Expression);
			Output.WriteLine ();
		}

		// FIXME: It is still a C#-ism
		protected override void GenerateIterationStatement( CodeIterationStatement statement) {
			TextWriter output = Output;

			output.Write( "for (" );
			GenerateStatement( statement.InitStatement );
			output.Write( "; " );
			
			GenerateExpression( statement.TestExpression );
			output.Write( "; " );
			GenerateStatement( statement.IncrementStatement );
			
			output.Write( ": " );
			
			GenerateStatements( statement.Statements );
			if ((statement.Statements.Count) == 0 ){
				output.WriteLine ("pass");
			}
		}

		protected override void GenerateThrowExceptionStatement ( CodeThrowExceptionStatement statement)
		{
			Output.Write ("raise");
			if (statement.ToThrow != null) {
				Output.Write (' ');
				GenerateExpression (statement.ToThrow);
			}
			Output.WriteLine ();
		}

		protected override void GenerateComment (CodeComment comment)
		{
			TextWriter output = Output;
			string [] lines = comment.Text.Split ('\n');
			bool first = true;
			foreach (string line in lines ) {
				if ( comment.DocComment )
					output.Write ("#"); // no doc comment format in boo ( yet ?? )
				else
					output.Write ("#");
				if (first) {
					output.Write (' ');
					first = false;
				}
				output.WriteLine (line);
			}
		}

		// FIXME: How should I represent return statement?
		protected override void GenerateMethodReturnStatement ( 
			CodeMethodReturnStatement statement)
		{
			TextWriter output = Output;
			Output.Write("return ");
			if (statement.Expression != null)
				GenerateExpression (statement.Expression);
			output.WriteLine ();
		}

		protected override void GenerateConditionStatement (
			CodeConditionStatement statement)
		{
			TextWriter output = Output;
			output.Write( "if (" );

			GenerateExpression( statement.Condition);

			output.WriteLine( ") :" );
			++Indent;
			GenerateStatements( statement.TrueStatements );
			if ((statement.TrueStatements.Count) == 0 ){
				output.WriteLine ("pass");
			}
			--Indent;

			CodeStatementCollection falses = statement.FalseStatements;
			if ( falses.Count > 0 ) {
				//output.Write( '}' );
				if ( Options.ElseOnClosing )
					output.Write( ' ' );
				else
					output.WriteLine();
				output.WriteLine( "else :" );
				++Indent;
				GenerateStatements( falses );
				if ((falses.Count) == 0 ){
					output.WriteLine ("pass");
				}
				--Indent;
			}
		}

		protected override void GenerateTryCatchFinallyStatement ( 
			CodeTryCatchFinallyStatement statement)
		{
			TextWriter output = Output;
			CodeGeneratorOptions options = Options;

			output.WriteLine ("try :");
			++Indent;
			GenerateStatements (statement.TryStatements);
			if ((statement.TryStatements.Count) == 0 ){
				output.WriteLine ("pass");
			}
			--Indent;

			if (statement.CatchClauses.Count > 0) {
				
				output.WriteLine ();
				foreach (CodeCatchClause clause in statement.CatchClauses) {
					output.Write ("except ");
					OutputTypeNamePair(clause.CatchExceptionType, GetSafeName (clause.LocalName));
					output.WriteLine (":");
					++Indent;
					GenerateStatements (clause.Statements);
					if (clause.Statements.Count == 0 )
					{
						output.WriteLine ("pass");
					}
					--Indent;
				}
			}

			CodeStatementCollection finallies = statement.FinallyStatements;
			if ( finallies.Count > 0 ) {
				if ( options.ElseOnClosing )
					output.Write( ' ' );
				else
					output.WriteLine();
				output.WriteLine( "ensure :" );
				++Indent;
				GenerateStatements( finallies );
				if (finallies.Count == 0 )
				{
					output.WriteLine ("pass");
				}
				--Indent;
			}

			output.WriteLine();
		}

		protected override void GenerateAssignStatement (
			CodeAssignStatement statement)
		{
			TextWriter output = Output;
			GenerateExpression (statement.Left);
			output.Write (" = ");
			GenerateExpression (statement.Right);
			output.WriteLine ();
		}

		protected override void GenerateAttachEventStatement (
			CodeAttachEventStatement statement)
		{
			TextWriter output = Output;

			GenerateEventReferenceExpression (statement.Event);
			output.Write (" += ");
			GenerateExpression (statement.Listener);
			output.WriteLine ();
		}

		protected override void GenerateRemoveEventStatement (
			CodeRemoveEventStatement statement)
		{
			TextWriter output = Output;
			GenerateEventReferenceExpression (statement.Event);
			Output.Write (" -= ");
			GenerateExpression (statement.Listener);
			output.WriteLine();
		}

		// FIXME: do something after it gets supported.
		protected override void GenerateGotoStatement (
			CodeGotoStatement statement)
		{
			throw new NotSupportedException ("Goto statement is not supported in Boo.");
		}
		
		protected override void GenerateLabeledStatement (
			CodeLabeledStatement statement)
		{
			throw new NotSupportedException ("Labeled statement is not supported in Boo.");
		}

		protected override void GenerateVariableDeclarationStatement ( CodeVariableDeclarationStatement statement)
		{
			TextWriter output = Output;
		
			OutputTypeNamePair (statement.Type, GetSafeName(statement.Name));

			CodeExpression initExpression = statement.InitExpression;
			if (initExpression != null) {
				output.Write ( " = " );
				GenerateExpression( initExpression );
			}

			output.WriteLine( );
		}

		protected override void GenerateLinePragmaStart (CodeLinePragma linePragma)
		{
			// TODO - right now don't throw exception as this breaks asp.net support
			//throw new NotSupportedException ("Line pragma is not supported in Boo.");
		}

		protected override void GenerateLinePragmaEnd (CodeLinePragma linePragma)
		{
			//throw new NotSupportedException ("Line pragma is not supported in Boo.");			
		}

		protected override void GenerateEvent (CodeMemberEvent eventRef,
			CodeTypeDeclaration declaration )
		{
			OutputMemberAccessModifier (eventRef.Attributes);
			OutputMemberScopeModifier (eventRef.Attributes | MemberAttributes.Final); // Don't output "virtual"
			Output.Write ("event ");
		
			OutputTypeNamePair (eventRef.Type, GetSafeName (eventRef.Name));
			Output.WriteLine ();
		}

		protected override void GenerateField (CodeMemberField field)
		{
			TextWriter output = Output;

			if (field.CustomAttributes.Count > 0)
				OutputAttributeDeclarations( field.CustomAttributes );

			MemberAttributes attributes = field.Attributes;
			OutputMemberAccessModifier( attributes );
			OutputFieldScopeModifier( attributes );

			if (IsCurrentEnum)
				Output.Write(field.Name);
			else
				OutputTypeNamePair( field.Type, GetSafeName (field.Name) );

			CodeExpression initExpression = field.InitExpression;
			if ( initExpression != null ) {
				output.Write( " = " );
				GenerateExpression( initExpression );
			}

			if (IsCurrentEnum) {
				output.WriteLine( ',' );
			}			
		}
		
		protected override void GenerateSnippetMember (
			CodeSnippetTypeMember member)
		{
			Output.Write (member.Text);
		}
		
		protected override void GenerateEntryPointMethod (
			CodeEntryPointMethod method,
			CodeTypeDeclaration declaration)
		{
			method.Name = "__Main__";
			GenerateMethod (method, declaration);
		}
		protected override void GenerateMethod (CodeMemberMethod method,
			CodeTypeDeclaration declaration)
		{
			TextWriter output = Output;

			if (method.CustomAttributes.Count > 0)
				OutputAttributeDeclarations (method.CustomAttributes);

			if (method.ReturnTypeCustomAttributes.Count > 0)
				OutputAttributeDeclarations (method.ReturnTypeCustomAttributes);

			MemberAttributes attributes = method.Attributes;

			if (method.PrivateImplementationType == null && !declaration.IsInterface)
				OutputMemberAccessModifier (attributes);

			if (!declaration.IsInterface)
				OutputMemberScopeModifier (attributes);

			CodeTypeReference privateType =
				method.PrivateImplementationType;
			if (privateType != null) {
				OutputType (privateType);
				output.Write ('.');
			}
			output.Write("def ");
			output.Write (GetSafeName (method.Name));

			output.Write (' ');

			output.Write ('(');
			OutputParameters (method.Parameters);
			output.Write (')');

			output.Write (" as ");
			OutputType (method.ReturnType);
			
			if ( (attributes & MemberAttributes.ScopeMask) == MemberAttributes.Abstract || declaration.IsInterface) 
			{
				//output.WriteLine ();
			}
			else {
				output.WriteLine (" :");
				++Indent;
				GenerateStatements (method.Statements);
				if (method.Statements.Count == 0 ){
					output.WriteLine ("pass");
				}
				--Indent;
			}
		}
		
		protected override void GenerateProperty (
			CodeMemberProperty property,
			CodeTypeDeclaration declaration)
		{
			TextWriter output = Output;

			if (property.CustomAttributes.Count > 0)
				OutputAttributeDeclarations( property.CustomAttributes );

			MemberAttributes attributes = property.Attributes;
			OutputMemberAccessModifier( attributes );
			OutputMemberScopeModifier( attributes );


			if (property.Name == "Item")
			{
				// indexer
				output.Write(GetSafeName (property.Name));
				output.Write("(");
				OutputParameters(property.Parameters);
				output.Write(")");
				output.Write(" as ");
				OutputType(property.Type);
			}
			else
			{

				OutputTypeNamePair( property.Type, GetSafeName (property.Name));
			}
			output.WriteLine (" :");
			++Indent;

			if (declaration.IsInterface)
			{
				if (property.HasGet) output.WriteLine("get: ");
				if (property.HasSet) output.WriteLine("set: ");
			}
			else
			{
				if (property.HasGet)
				{
					output.WriteLine ("get :");
					++Indent;

					GenerateStatements (property.GetStatements);
					if ((property.GetStatements.Count) == 0 ){
						output.WriteLine ("pass");
					}
					--Indent;
				}

				if (property.HasSet)
				{
					output.WriteLine ("set :");
					++Indent;

					GenerateStatements (property.SetStatements);
					if (property.SetStatements.Count == 0){
						output.WriteLine ("pass");
					}
					--Indent;
				}
			}

			--Indent;			
		}

		protected override void GenerateConstructor( CodeConstructor constructor,
								 CodeTypeDeclaration declaration )
		{
			OutputMemberAccessModifier (constructor.Attributes);
			Output.Write ("def constructor(");
			OutputParameters (constructor.Parameters);
			Output.Write (") ");
			Output.WriteLine (":");
			Indent++;
			if (constructor.ChainedConstructorArgs.Count > 0)
			{
				Output.Write("this (");
				bool first = true;
				foreach (CodeExpression ex in constructor.ChainedConstructorArgs)
				{
					if (!first)
						Output.Write(", ");
					first = false;
					GenerateExpression(ex);
				}
				
				Output.Write(")");
			};
 			if (constructor.BaseConstructorArgs.Count > 0) {
				Output.Write("super(");
				bool first = true;
				foreach (CodeExpression ex in constructor.BaseConstructorArgs)
				{
					if (!first)
						Output.Write(", ");
					first = false;
					GenerateExpression(ex);
				}
				
				Output.WriteLine (")");
			};
			GenerateStatements (constructor.Statements);
			if ((constructor.Statements.Count) == 0 ){
				Output.WriteLine ("pass");
			}
			Indent--;			
		}
		
		protected override void GenerateTypeConstructor (
			CodeTypeConstructor constructor )
		{
			Output.WriteLine ("static constructor() :");
			Indent++;
			GenerateStatements (constructor.Statements);
			if ((constructor.Statements.Count) == 0 ){
				Output.WriteLine ("pass");
			}
			Indent--;			
		}

		protected override void GenerateTypeStart (
			CodeTypeDeclaration declaration)
		{
			TextWriter output = Output;
			CodeTypeDelegate del = declaration as CodeTypeDelegate;
			
			if (declaration.CustomAttributes.Count > 0)
				OutputAttributeDeclarations ( declaration.CustomAttributes );

			TypeAttributes attributes = declaration.TypeAttributes;
			

			if (del != null) {
				LocalOutputTypeAttributes( attributes,
						declaration.IsStruct,
						 declaration.IsEnum );
				output.Write("callable ");
				output.Write (GetSafeName (declaration.Name));
				output.Write( '(' );
				OutputParameters(del.Parameters);
				output.Write( ')' );
			
			} else {
				OutputTypeAttributes (attributes,
					declaration.IsStruct,
					 declaration.IsEnum );
				output.Write (GetSafeName (declaration.Name));
				output.Write( '(' );
				
				IEnumerator enumerator = declaration.BaseTypes.GetEnumerator();
				if ( enumerator.MoveNext() ) {
			CodeTypeReference type = (CodeTypeReference)enumerator.Current;
				
					
				currentType = GetTypeOutput(type);
					OutputType( type );
					
					while ( enumerator.MoveNext() ) {
						type = (CodeTypeReference)enumerator.Current;
					
						output.Write( ", " );
						OutputType( type );
					}

					output.Write( ')' );
				}
				output.WriteLine ( ":" );
			}
			++Indent;
		}

		protected override void GenerateTypeEnd ( CodeTypeDeclaration declaration )
		{			
			if (declaration is CodeTypeDelegate) {
				Output.WriteLine ( );
			} 			
			--Indent;
			currentType = "";	
		}

		protected override void GenerateNamespaceStart ( CodeNamespace ns )
		{
			TextWriter output = Output;
			
			string name = ns.Name;
			if ( name != null && name != "" ) {
				output.Write( "namespace " );
				output.Write( GetSafeName (name) );
				output.WriteLine( );				
			}
		}

		protected override void GenerateNamespaceEnd ( CodeNamespace ns )
		{
			string name = ns.Name;
			if ( name != null && name != "" ) {
				--Indent;				
			}
		}

		protected override void GenerateNamespaceImport ( CodeNamespaceImport import )
		{
			TextWriter output = Output;

			output.Write( "import " );
			output.Write( GetSafeName (import.Namespace) );
			output.WriteLine( );
		}

		protected override void GenerateAttributeDeclarationsStart ( CodeAttributeDeclarationCollection attributes )
		{
			Output.Write( '[' );
			CodeMemberMethod met = CurrentMember as CodeMemberMethod;
			if (met != null && met.ReturnTypeCustomAttributes == attributes)
				Output.Write ("return: ");
		}
		
		protected override void GenerateAttributeDeclarationsEnd( CodeAttributeDeclarationCollection attributes )
		{
			Output.WriteLine( ']' );
		}

		protected override void OutputType (CodeTypeReference type)
		{
			Output.Write (GetTypeOutput(type));
		}

		protected override void OutputTypeNamePair (
			CodeTypeReference type, string name)
		{
			Output.Write (GetSafeName (name));
			Output.Write (" as ");
			Output.Write (GetTypeOutput (type));
		}

		protected override string QuoteSnippetString ( string value )
		{
			// FIXME: this is weird, but works.
			string output = value.Replace ("\\", "\\\\");
			output = output.Replace ("\"", "\\\"");
			output = output.Replace ("\t", "\\t");
			output = output.Replace ("\r", "\\r");
			output = output.Replace ("\n", "\\n");

			return "\"" + output + "\"";
		}

		private void GenerateDeclaration (CodeTypeReference type,
			string name, CodeExpression initExpression)
		{
			TextWriter output = Output;

			OutputTypeNamePair (type, GetSafeName (name));

			if ( initExpression != null ) {
				output.Write( " = " );
				GenerateExpression( initExpression );
			}

			output.WriteLine();
		}
		
		private void GenerateMemberReferenceExpression( CodeExpression targetObject, string memberName )
		{
			if (targetObject != null ) {
				GenerateExpression( targetObject );
				Output.Write( '.' );
			}
			Output.Write( GetSafeName (memberName) );
		}
			
		protected override void GenerateParameterDeclarationExpression (
			CodeParameterDeclarationExpression e)
		{
			if (e.CustomAttributes != null && e.CustomAttributes.Count > 0)
				OutputAttributeDeclarations (e.CustomAttributes);
			OutputDirection (e.Direction);
			Output.Write (GetSafeName (e.Name));
			Output.Write (" as ");
			OutputType (e.Type);
		}

		protected override void GenerateTypeOfExpression (
			CodeTypeOfExpression e)
		{
			Output.Write ("typeof(");
			OutputType (e.Type);
			Output.Write (")");
		}

		/* 
		 * ICodeGenerator
		 */

		protected override string CreateEscapedIdentifier (string value)
		{
			if (value == null)
				throw new NullReferenceException ("Argument identifier is null.");
			return GetSafeName (value);
		}

		protected override string CreateValidIdentifier (string value)
		{
			if (value == null)
				throw new NullReferenceException ();

			if (keywordsTable == null)
				FillKeywordTable ();

			if (keywordsTable.Contains (value) || typesTable.Contains (value))
				return "_" + value;
			else
				return value;
		}

		protected override string GetTypeOutput( CodeTypeReference type )
		{
			string output;
			CodeTypeReference arrayType;

			arrayType = type.ArrayElementType;
			if ( arrayType != null ) {
				output = GetTypeOutput( arrayType );
			}
			else { 
				switch ( type.BaseType ) {
				case "System.Double":
					output = "double";
					break;
				case "System.Single":
					output = "float";
					break;
				case "System.Int32":
					output = "int";
					break;
				case "System.Int64":
					output = "long";
					break;
				case "System.Int16":
					output = "short";
					break;
				case "System.UInt16":
					output = "ushort";
					break;
				case "System.Boolean":
					output = "bool";
					break;
				/*case "System.Char":
					output = "char";
					break;*/
				case "System.String":
					output = "string";
					break;
				case "System.Object":
					output = "object";
					break;

				case "System.Void":
					output = "void";
					break;

				default:
					output = GetSafeTypeName (type.BaseType);
					break;
				}
			}

			int rank = type.ArrayRank;
			if ( rank > 0 ) {
				output += "[";
				for ( --rank; rank > 0; --rank  )
					output += ",";
				output += "]";
			}

			return output;
		}

		protected override bool IsValidIdentifier ( string identifier )
		{
			if (keywordsTable == null)
				FillKeywordTable ();

			return !keywordsTable.Contains (identifier) && !typesTable.Contains (identifier);
		}

		protected override bool Supports( GeneratorSupport supports )
		{
			if ( (supports & GeneratorSupport.Win32Resources) != 0 )
				return false;
			return true;
		}

		string GetSafeName (string id)
		{
			if (keywordsTable == null) {
				FillKeywordTable ();
			}
			if (keywordsTable.Contains (id) || typesTable.Contains (id)) return "@" + id;
			else return id;
		}

		string GetSafeTypeName (string id)
		{
			if (keywordsTable == null) {
				FillKeywordTable ();
			}   
			if (keywordsTable.Contains (id)) return "@" + id;
			else return id;
		}

		static void FillKeywordTable ()
		{
			keywordsTable = new Hashtable ();
				foreach (string keyword in keywords) keywordsTable.Add (keyword,keyword);
			typesTable = new Hashtable ();
				foreach (string type in types) typesTable.Add (type,type);
		}
		protected void LocalOutputTypeAttributes(TypeAttributes attributes, bool isStruct, bool isEnum)
		{
			TypeAttributes attributes1 = (attributes & TypeAttributes.NestedFamORAssem);
			switch (attributes1)
			{
				case TypeAttributes.Public:
					case TypeAttributes.NestedPublic:
				{
					this.Output.Write("public ");
					break;
				}
				case TypeAttributes.NestedPrivate:
				{
					this.Output.Write("private ");
					break;
				}
			}
		}

		static Hashtable typesTable;
		static string[] types = new string[] {
			"object","bool","byte","float",
			"int","short","double","long","string","void"
		};

		static Hashtable keywordsTable;
		static string[] keywords = new string[] {
		"abstract", "and", "as", "break", "continue", "callable", "cast", "class", "constructor", "def", "else", "ensure", "enum", 
		"except", "failure", "final", "from", "for", "false", "get", "given", "import", "interface", "internal", "is", "isa", 
		"if", "in", "not", "null", "or", "otherwise", "override", "pass", "namespace", "public", "protected", "private", "raise", 
		"return", "retry", "set","self", "super", "static", "success", "try", "transient", "true", "typeof", "unless", "virtual", 
		"when", "while", "yield"
		};
	}
}
