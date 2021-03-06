﻿#region license
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

using System.Collections;
using Boo.Lang.Compiler.Ast;
using Boo.Lang.Compiler.Steps.Generators;
using Boo.Lang.Compiler.TypeSystem;
using Boo.Lang.Compiler.TypeSystem.Internal;
using Boo.Lang.Compiler.Util;

namespace Boo.Lang.Compiler.Steps
{
	public class ProcessGenerators : AbstractTransformerCompilerStep
	{
		public static readonly System.Reflection.ConstructorInfo List_IEnumerableConstructor = Methods.ConstructorOf(() => new List(default(IEnumerable)));

	    private Method _current;
		
		public override void Run()
		{
            if (Errors.Count > 0) return;
			Visit(CompileUnit.Modules);
		}
		
		public override void OnInterfaceDefinition(InterfaceDefinition node)
		{
			// ignore
		}
		
		public override void OnEnumDefinition(EnumDefinition node)
		{
			// ignore
		}
		
		public override void OnField(Field node)
		{
			// ignore
		}
		
		public override void OnConstructor(Constructor method)
		{
			_current = method;
			Visit(_current.Body);
		}
		
		public override bool EnterMethod(Method method)
		{
			_current = method;
			return true;
		}
		
		public override void LeaveMethod(Method method)
		{
			var entity = (InternalMethod)method.Entity;
			if (!entity.IsGenerator) return;

			var processor = new GeneratorMethodProcessor(Context, entity);
			processor.Run();
		}

		public override void OnListLiteralExpression(ListLiteralExpression node)
		{
			var generator = AstUtil.IsListGenerator(node);
			Visit(node.Items);
			if (generator)
				ReplaceCurrentNode(
					CodeBuilder.CreateConstructorInvocation(
						TypeSystemServices.Map(List_IEnumerableConstructor),
						node.Items[0]));
		}
		
		public override void LeaveGeneratorExpression(GeneratorExpression node)
		{
			var collector = new ForeignReferenceCollector { CurrentType = TypeContaining(node) };
			node.Accept(collector);

			var processor = new GeneratorExpressionProcessor(Context, collector, node);
			processor.Run();

			ReplaceCurrentNode(processor.CreateEnumerableConstructorInvocation());
		}

		private static IType TypeContaining(GeneratorExpression node)
		{
			return (IType) AstUtil.GetParentClass(node).Entity;
		}
	}
}
