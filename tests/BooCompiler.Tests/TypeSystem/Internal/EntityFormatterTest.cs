﻿using Boo.Lang.Compiler.Ast;
using Boo.Lang.Compiler.TypeSystem;
using Boo.Lang.Compiler.TypeSystem.Builders;
using Boo.Lang.Environments;
using NUnit.Framework;

namespace BooCompiler.Tests.TypeSystem.Internal
{
	[TestFixture]
	public class EntityFormatterTest : EntityFormatterTestBase
	{
		protected override IType SimpleType()
		{	
			return InvokeInCompilerContextEnvironment(() => DefineInternalClass("Foo", "Bar"));
		}

		protected override IType CallableType()
		{
			return (IType)InvokeInCompilerContextEnvironment(() => My<CallableTypeBuilder>.Instance.ForCallableDefinition(new CallableDefinition { Name = "Foo" }).Entity);
		}

		protected override IType GenericType()
		{
			return InvokeInCompilerContextEnvironment(() => DefineGenericType());
		}

		private IType DefineGenericType()
		{
			var type = BuildInternalClass("Foo", "Bar");
			type.AddGenericParameter("T");
			return type.Entity;
		}

		protected override IType ArrayType()
		{
			return SimpleType().MakeArrayType(1);
		}
	}
}
