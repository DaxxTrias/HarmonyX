using HarmonyLib;
using HarmonyLibTests.Assets;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using static HarmonyLibTests.Assets.AccessToolsMethodDelegate;

namespace HarmonyLibTests
{
	[TestFixture]
	public class Test_AccessTools : TestLogger
	{
		[Test]
		public void Test_AccessTools_Field1()
		{
			var type = typeof(AccessToolsClass);

			Assert.Null(AccessTools.DeclaredField(null, null));
			Assert.Null(AccessTools.DeclaredField(type, null));
			Assert.Null(AccessTools.DeclaredField(null, "field1"));
			Assert.Null(AccessTools.DeclaredField(type, "unknown"));

			var field = AccessTools.DeclaredField(type, "field1");
			Assert.NotNull(field);
			Assert.AreEqual(type, field.DeclaringType);
			Assert.AreEqual("field1", field.Name);
		}

		[Test]
		public void Test_AccessTools_Field2()
		{
			var type = typeof(AccessToolsClass);
			Assert.NotNull(AccessTools.Field(type, "field1"));
			Assert.NotNull(AccessTools.DeclaredField(type, "field1"));

			var subtype = typeof(AccessToolsSubClass);
			Assert.NotNull(AccessTools.Field(subtype, "field1"));
			Assert.Null(AccessTools.DeclaredField(subtype, "field1"));
		}

		[Test]
		public void Test_AccessTools_Property1()
		{
			var type = typeof(AccessToolsClass);

			Assert.Null(AccessTools.Property(null, null));
			Assert.Null(AccessTools.Property(type, null));
			Assert.Null(AccessTools.Property(null, "Property"));
			Assert.Null(AccessTools.Property(type, "unknown"));

			var prop = AccessTools.Property(type, "Property");
			Assert.NotNull(prop);
			Assert.AreEqual(type, prop.DeclaringType);
			Assert.AreEqual("Property", prop.Name);
		}

		[Test]
		public void Test_AccessTools_Property2()
		{
			var type = typeof(AccessToolsClass);
			Assert.NotNull(AccessTools.Property(type, "Property"));
			Assert.NotNull(AccessTools.DeclaredProperty(type, "Property"));

			var subtype = typeof(AccessToolsSubClass);
			Assert.NotNull(AccessTools.Property(subtype, "Property"));
			Assert.Null(AccessTools.DeclaredProperty(subtype, "Property"));
		}

		[Test]
		public void Test_AccessTools_Method1()
		{
			var type = typeof(AccessToolsClass);

			Assert.Null(AccessTools.Method(null));
			Assert.Null(AccessTools.Method(type, null));
			Assert.Null(AccessTools.Method(null, "Method1"));
			Assert.Null(AccessTools.Method(type, "unknown"));

			var m1 = AccessTools.Method(type, "Method1");
			Assert.NotNull(m1);
			Assert.AreEqual(type, m1.DeclaringType);
			Assert.AreEqual("Method1", m1.Name);

			var m2 = AccessTools.Method("HarmonyLibTests.Assets.AccessToolsClass:Method1");
			Assert.NotNull(m2);
			Assert.AreEqual(type, m2.DeclaringType);
			Assert.AreEqual("Method1", m2.Name);

			var m3 = AccessTools.Method(type, "Method1", new Type[] { });
			Assert.NotNull(m3);

			var m4 = AccessTools.Method(type, "SetField", new Type[] { typeof(string) });
			Assert.NotNull(m4);
		}

		[Test]
		public void Test_AccessTools_Method2()
		{
			var type = typeof(AccessToolsSubClass);

			var m1 = AccessTools.Method(type, "Method1");
			Assert.NotNull(m1);

			var m2 = AccessTools.DeclaredMethod(type, "Method1");
			Assert.Null(m2);
		}

		[Test]
		public void Test_AccessTools_InnerClass()
		{
			var type = typeof(AccessToolsClass);

			Assert.Null(AccessTools.Inner(null, null));
			Assert.Null(AccessTools.Inner(type, null));
			Assert.Null(AccessTools.Inner(null, "Inner"));
			Assert.Null(AccessTools.Inner(type, "unknown"));

			var cls = AccessTools.Inner(type, "Inner");
			Assert.NotNull(cls);
			Assert.AreEqual(type, cls.DeclaringType);
			Assert.AreEqual("Inner", cls.Name);
		}

		[Test]
		public void Test_AccessTools_GetTypes()
		{
			var empty = AccessTools.GetTypes(null);
			Assert.NotNull(empty);
			Assert.AreEqual(0, empty.Length);

			// TODO: typeof(null) is ambiguous and resolves for now to <object>. is this a problem?
			var types = AccessTools.GetTypes(new object[] { "hi", 123, null, new Test_AccessTools() });
			Assert.NotNull(types);
			Assert.AreEqual(4, types.Length);
			Assert.AreEqual(typeof(string), types[0]);
			Assert.AreEqual(typeof(int), types[1]);
			Assert.AreEqual(typeof(object), types[2]);
			Assert.AreEqual(typeof(Test_AccessTools), types[3]);
		}

		[Test]
		public void Test_AccessTools_GetDefaultValue()
		{
			Assert.AreEqual(null, AccessTools.GetDefaultValue(null));
			Assert.AreEqual((float)0, AccessTools.GetDefaultValue(typeof(float)));
			Assert.AreEqual(null, AccessTools.GetDefaultValue(typeof(string)));
			Assert.AreEqual(BindingFlags.Default, AccessTools.GetDefaultValue(typeof(BindingFlags)));
			Assert.AreEqual(null, AccessTools.GetDefaultValue(typeof(IEnumerable<bool>)));
			Assert.AreEqual(null, AccessTools.GetDefaultValue(typeof(void)));
		}

		[Test]
		public void Test_AccessTools_TypeExtension_Description()
		{
			var types = new Type[] { typeof(string), typeof(int), null, typeof(void), typeof(Test_AccessTools) };
			Assert.AreEqual("(System.String, System.Int32, null, System.Void, HarmonyLibTests.Test_AccessTools)", types.Description());
		}

		[Test]
		public void Test_AccessTools_TypeExtension_Types()
		{
			// public static void Resize<T>(ref T[] array, int newSize);
			var method = typeof(Array).GetMethod("Resize");
			var pinfo = method.GetParameters();
			var types = pinfo.Types();

			Assert.NotNull(types);
			Assert.AreEqual(2, types.Length);
			Assert.AreEqual(pinfo[0].ParameterType, types[0]);
			Assert.AreEqual(pinfo[1].ParameterType, types[1]);
		}

		[Test]
		public void Test_AccessTools_FieldRefAccess_ByName()
		{
			var fieldInfo = typeof(AccessToolsClass).GetField("field1", BindingFlags.Instance | BindingFlags.NonPublic);
			Assert.NotNull(fieldInfo);
			var instance = new AccessToolsClass();
			var fieldRef = AccessTools.FieldRefAccess<AccessToolsClass, string>("field1");
			ref var value = ref fieldRef(instance);

			Assert.AreEqual(AccessToolsClass.field1Value, value);
			var newValue = AccessToolsClass.field1Value + "1";
			value = newValue;
			Assert.AreEqual(newValue, fieldInfo.GetValue(instance));
		}

		[Test]
		public void Test_AccessTools_FieldRefAccess_ByFieldInfo()
		{
			var fieldInfo = typeof(AccessToolsClass).GetField("field1", BindingFlags.Instance | BindingFlags.NonPublic);
			Assert.NotNull(fieldInfo);
			var instance = new AccessToolsClass();
			var fieldRef = AccessTools.FieldRefAccess<AccessToolsClass, string>(fieldInfo);
			ref var value = ref fieldRef(instance);

			Assert.AreEqual(AccessToolsClass.field1Value, value);
			var newValue = AccessToolsClass.field1Value + "1";
			value = newValue;
			Assert.AreEqual(newValue, fieldInfo.GetValue(instance));
		}

		[Test]
		public void Test_AccessTools_FieldRefAccess_ByFieldInfo_Readonly()
		{
			var fieldInfo = typeof(AccessToolsClass).GetField("field2", BindingFlags.Instance | BindingFlags.NonPublic);
			Assert.NotNull(fieldInfo);
			var instance = new AccessToolsClass();
			var fieldRef = AccessTools.FieldRefAccess<AccessToolsClass, string>(fieldInfo);
			ref var value = ref fieldRef(instance);

			Assert.AreEqual(AccessToolsClass.field2Value, value);
			var newValue = AccessToolsClass.field2Value + "1";
			value = newValue;
			Assert.AreEqual(newValue, fieldInfo.GetValue(instance));
		}

		[Test]
		public void Test_AccessTools_StaticFieldRefAccess_ByName()
		{
			var fieldInfo = typeof(AccessToolsClass).GetField("field3", BindingFlags.Static | BindingFlags.NonPublic);
			Assert.NotNull(fieldInfo);
			var instance = new AccessToolsClass();
			ref var value = ref AccessTools.StaticFieldRefAccess<AccessToolsClass, string>("field3");

			Assert.AreEqual(AccessToolsClass.field3Value, value);
			var newValue = AccessToolsClass.field3Value + "1";
			value = newValue;
			Assert.AreEqual(newValue, fieldInfo.GetValue(instance));
		}

		[Test]
		public void Test_AccessTools_StaticFieldRefAccess_ByFieldInfo()
		{
			var fieldInfo = typeof(AccessToolsClass).GetField("field3", BindingFlags.Static | BindingFlags.NonPublic);
			Assert.NotNull(fieldInfo);
			var instance = new AccessToolsClass();
			var fieldRef = AccessTools.StaticFieldRefAccess<string>(fieldInfo);
			ref var value = ref fieldRef();

			Assert.AreEqual(AccessToolsClass.field3Value, value);
			var newValue = AccessToolsClass.field3Value + "1";
			value = newValue;
			Assert.AreEqual(newValue, fieldInfo.GetValue(instance));
		}

		[Test]
		public void Test_AccessTools_StaticFieldRefAccess_ByFieldInfo_Readonly()
		{
			var fieldInfo = typeof(AccessToolsClass).GetField("field4", BindingFlags.Static | BindingFlags.NonPublic);
			Assert.NotNull(fieldInfo);
			var instance = new AccessToolsClass();
			var fieldRef = AccessTools.FieldRefAccess<AccessToolsClass, string>(fieldInfo);
			ref var value = ref fieldRef(instance);

			Assert.AreEqual(AccessToolsClass.field4Value, value);
			var newValue = AccessToolsClass.field4Value + "1";
			value = newValue;
			Assert.AreEqual(newValue, fieldInfo.GetValue(instance));
		}

		[Test]
		public void Test_AccessTools_FieldRefAccess_ByFieldInfo_SubClass()
		{
			var fieldInfo = typeof(AccessToolsClass).GetField("field1", BindingFlags.Instance | BindingFlags.NonPublic);
			Assert.NotNull(fieldInfo);
			var instance = new AccessToolsSubClass();
			var fieldRef = AccessTools.FieldRefAccess<AccessToolsSubClass, string>(fieldInfo);
			ref var value = ref fieldRef(instance);

			Assert.AreEqual(AccessToolsClass.field1Value, value);
			var newValue = AccessToolsClass.field1Value + "1";
			value = newValue;
			Assert.AreEqual(newValue, fieldInfo.GetValue(instance));
		}

		[Test]
		public void Test_AccessTools_FieldRefAccess_Anonymous()
		{
			var fieldInfo = typeof(AccessToolsClass).GetField("field1", BindingFlags.Instance | BindingFlags.NonPublic);
			Assert.NotNull(fieldInfo);
			var instance = new AccessToolsClass();
			var fieldRef = AccessTools.FieldRefAccess<object, string>(fieldInfo);
			ref var value = ref fieldRef(instance);

			Assert.AreEqual(AccessToolsClass.field1Value, value);
			var newValue = AccessToolsClass.field1Value + "1";
			value = newValue;
			Assert.AreEqual(newValue, fieldInfo.GetValue(instance));
		}

		private static readonly MethodInfo interfaceTest = typeof(IInterface).GetMethod("Test");
		private static readonly MethodInfo baseTest = typeof(Base).GetMethod("Test");
		private static readonly MethodInfo derivedTest = typeof(Derived).GetMethod("Test");
		private static readonly MethodInfo structTest = typeof(Struct).GetMethod("Test");
		private static readonly MethodInfo staticTest = typeof(AccessToolsMethodDelegate).GetMethod("Test");

		[Test]
		public void Test_AccessTools_MethodDelegate_ClosedInstanceDelegates()
		{
			var f = 789f;
			var baseInstance = new Base();
			var derivedInstance = new Derived();
			var structInstance = new Struct();
			Assert.AreEqual("base test 456 790 1", AccessTools.MethodDelegate<MethodDel>(baseTest, baseInstance, virtualCall: true)(456, ref f));
			Assert.AreEqual("base test 456 791 2", AccessTools.MethodDelegate<MethodDel>(baseTest, baseInstance, virtualCall: false)(456, ref f));
			Assert.AreEqual("derived test 456 792 1", AccessTools.MethodDelegate<MethodDel>(baseTest, derivedInstance, virtualCall: true)(456, ref f));
			Assert.AreEqual("base test 456 793 2", AccessTools.MethodDelegate<MethodDel>(baseTest, derivedInstance, virtualCall: false)(456, ref f));
			// derivedTest => baseTest automatically for virtual calls
			Assert.AreEqual("base test 456 794 3", AccessTools.MethodDelegate<MethodDel>(derivedTest, baseInstance, virtualCall: true)(456, ref f));
			_ = Assert.Throws(typeof(ArgumentException), () => AccessTools.MethodDelegate<MethodDel>(derivedTest, baseInstance, virtualCall: false)(456, ref f));
			Assert.AreEqual("derived test 456 795 3", AccessTools.MethodDelegate<MethodDel>(derivedTest, derivedInstance, virtualCall: true)(456, ref f));
			Assert.AreEqual("derived test 456 796 4", AccessTools.MethodDelegate<MethodDel>(derivedTest, derivedInstance, virtualCall: false)(456, ref f));
			Assert.AreEqual("struct result 456 797 1", AccessTools.MethodDelegate<MethodDel>(structTest, structInstance, virtualCall: true)(456, ref f));
			Assert.AreEqual("struct result 456 798 1", AccessTools.MethodDelegate<MethodDel>(structTest, structInstance, virtualCall: false)(456, ref f));
		}

		[Test]
		public void Test_AccessTools_MethodDelegate_ClosedInstanceDelegates_InterfaceMethod()
		{
			var f = 789f;
			var baseInstance = new Base();
			var derivedInstance = new Derived();
			var structInstance = new Struct();
			Assert.AreEqual("base test 456 790 1", AccessTools.MethodDelegate<MethodDel>(interfaceTest, baseInstance, virtualCall: true)(456, ref f));
			_ = Assert.Throws(typeof(ArgumentException), () => AccessTools.MethodDelegate<MethodDel>(interfaceTest, baseInstance, virtualCall: false)(456, ref f));
			Assert.AreEqual("derived test 456 791 1", AccessTools.MethodDelegate<MethodDel>(interfaceTest, derivedInstance, virtualCall: true)(456, ref f));
			_ = Assert.Throws(typeof(ArgumentException), () => AccessTools.MethodDelegate<MethodDel>(interfaceTest, derivedInstance, virtualCall: false)(456, ref f));
			Assert.AreEqual("struct result 456 792 1", AccessTools.MethodDelegate<MethodDel>(interfaceTest, structInstance, virtualCall: true)(456, ref f));
			_ = Assert.Throws(typeof(ArgumentException), () => AccessTools.MethodDelegate<MethodDel>(interfaceTest, structInstance, virtualCall: false)(456, ref f));
		}

		[Test]
		public void Test_AccessTools_MethodDelegate_OpenInstanceDelegates()
		{
			var f = 789f;
			var baseInstance = new Base();
			var derivedInstance = new Derived();
			var structInstance = new Struct();
			Assert.AreEqual("base test 456 790 1", AccessTools.MethodDelegate<OpenMethodDel<Base>>(baseTest, virtualCall: true)(baseInstance, 456, ref f));
			Assert.AreEqual("base test 456 791 2", AccessTools.MethodDelegate<OpenMethodDel<Base>>(baseTest, virtualCall: false)(baseInstance, 456, ref f));
			Assert.AreEqual("derived test 456 792 1", AccessTools.MethodDelegate<OpenMethodDel<Base>>(baseTest, virtualCall: true)(derivedInstance, 456, ref f));
			Assert.AreEqual("base test 456 793 2", AccessTools.MethodDelegate<OpenMethodDel<Base>>(baseTest, virtualCall: false)(derivedInstance, 456, ref f));
			// derivedTest => baseTest automatically for virtual calls
			Assert.AreEqual("base test 456 794 3", AccessTools.MethodDelegate<OpenMethodDel<Base>>(derivedTest, virtualCall: true)(baseInstance, 456, ref f));
			_ = Assert.Throws(typeof(ArgumentException), () => AccessTools.MethodDelegate<OpenMethodDel<Base>>(derivedTest, virtualCall: false)(baseInstance, 456, ref f));
			Assert.AreEqual("derived test 456 795 3", AccessTools.MethodDelegate<OpenMethodDel<Base>>(derivedTest, virtualCall: true)(derivedInstance, 456, ref f));
			_ = Assert.Throws(typeof(ArgumentException), () => AccessTools.MethodDelegate<OpenMethodDel<Base>>(derivedTest, virtualCall: false)(derivedInstance, 456, ref f));
			// AccessTools.MethodDelegate<OpenMethodDel<Derived>>(derivedTest)(baseInstance, 456, ref f); // expected compile error
			// AccessTools.MethodDelegate<OpenMethodDel<Derived>>(derivedTest, virtualCall: false)(baseInstance, 456, ref f); // expected compile error
			Assert.AreEqual("derived test 456 796 4", AccessTools.MethodDelegate<OpenMethodDel<Derived>>(derivedTest, virtualCall: true)(derivedInstance, 456, ref f));
			Assert.AreEqual("derived test 456 797 5", AccessTools.MethodDelegate<OpenMethodDel<Derived>>(derivedTest, virtualCall: false)(derivedInstance, 456, ref f));
			Assert.AreEqual("struct result 456 798 1", AccessTools.MethodDelegate<OpenMethodDel<Struct>>(structTest, virtualCall: true)(structInstance, 456, ref f));
			Assert.AreEqual("struct result 456 799 1", AccessTools.MethodDelegate<OpenMethodDel<Struct>>(structTest, virtualCall: false)(structInstance, 456, ref f));
		}

		[Test]
		public void Test_AccessTools_MethodDelegate_OpenInstanceDelegates_DelegateInterfaceInstanceType()
		{
			var f = 789f;
			var baseInstance = new Base();
			var derivedInstance = new Derived();
			var structInstance = new Struct();
			Assert.AreEqual("base test 456 790 1", AccessTools.MethodDelegate<OpenMethodDel<IInterface>>(baseTest, virtualCall: true)(baseInstance, 456, ref f));
			_ = Assert.Throws(typeof(ArgumentException), () => AccessTools.MethodDelegate<OpenMethodDel<IInterface>>(baseTest, virtualCall: false)(baseInstance, 456, ref f));
			Assert.AreEqual("derived test 456 791 1", AccessTools.MethodDelegate<OpenMethodDel<IInterface>>(baseTest, virtualCall: true)(derivedInstance, 456, ref f));
			_ = Assert.Throws(typeof(ArgumentException), () => AccessTools.MethodDelegate<OpenMethodDel<IInterface>>(baseTest, virtualCall: false)(derivedInstance, 456, ref f));
			Assert.AreEqual("base test 456 792 2", AccessTools.MethodDelegate<OpenMethodDel<IInterface>>(derivedTest, virtualCall: true)(baseInstance, 456, ref f));
			_ = Assert.Throws(typeof(ArgumentException), () => AccessTools.MethodDelegate<OpenMethodDel<IInterface>>(derivedTest, virtualCall: false)(baseInstance, 456, ref f));
			Assert.AreEqual("derived test 456 793 2", AccessTools.MethodDelegate<OpenMethodDel<IInterface>>(derivedTest, virtualCall: true)(derivedInstance, 456, ref f));
			_ = Assert.Throws(typeof(ArgumentException), () => AccessTools.MethodDelegate<OpenMethodDel<IInterface>>(derivedTest, virtualCall: false)(derivedInstance, 456, ref f));
			Assert.AreEqual("struct result 456 794 1", AccessTools.MethodDelegate<OpenMethodDel<IInterface>>(structTest, virtualCall: true)(structInstance, 456, ref f));
			_ = Assert.Throws(typeof(ArgumentException), () => AccessTools.MethodDelegate<OpenMethodDel<IInterface>>(structTest, virtualCall: false)(structInstance, 456, ref f));
		}

		[Test]
		public void Test_AccessTools_MethodDelegate_OpenInstanceDelegates_InterfaceMethod()
		{
			var f = 789f;
			var baseInstance = new Base();
			var derivedInstance = new Derived();
			var structInstance = new Struct();
			Assert.AreEqual("base test 456 790 1", AccessTools.MethodDelegate<OpenMethodDel<IInterface>>(interfaceTest, virtualCall: true)(baseInstance, 456, ref f));
			_ = Assert.Throws(typeof(ArgumentException), () => AccessTools.MethodDelegate<OpenMethodDel<IInterface>>(interfaceTest, virtualCall: false)(baseInstance, 456, ref f));
			Assert.AreEqual("derived test 456 791 1", AccessTools.MethodDelegate<OpenMethodDel<IInterface>>(interfaceTest, virtualCall: true)(derivedInstance, 456, ref f));
			_ = Assert.Throws(typeof(ArgumentException), () => AccessTools.MethodDelegate<OpenMethodDel<IInterface>>(interfaceTest, virtualCall: false)(derivedInstance, 456, ref f));
			Assert.AreEqual("struct result 456 792 1", AccessTools.MethodDelegate<OpenMethodDel<IInterface>>(interfaceTest, virtualCall: true)(structInstance, 456, ref f));
			_ = Assert.Throws(typeof(ArgumentException), () => AccessTools.MethodDelegate<OpenMethodDel<IInterface>>(interfaceTest, virtualCall: false)(structInstance, 456, ref f));
			Assert.AreEqual("base test 456 793 2", AccessTools.MethodDelegate<OpenMethodDel<Base>>(interfaceTest, virtualCall: true)(baseInstance, 456, ref f));
			_ = Assert.Throws(typeof(ArgumentException), () => AccessTools.MethodDelegate<OpenMethodDel<Base>>(interfaceTest, virtualCall: false)(baseInstance, 456, ref f));
			Assert.AreEqual("derived test 456 794 2", AccessTools.MethodDelegate<OpenMethodDel<Base>>(interfaceTest, virtualCall: true)(derivedInstance, 456, ref f));
			_ = Assert.Throws(typeof(ArgumentException), () => AccessTools.MethodDelegate<OpenMethodDel<Base>>(interfaceTest, virtualCall: false)(derivedInstance, 456, ref f));
			// AccessTools.MethodDelegate<OpenMethodDel<Derived>>(interfaceTest, virtualCall: true)(baseInstance, 456, ref f)); // expected compile error
			// AccessTools.MethodDelegate<OpenMethodDel<Derived>>(interfaceTest, virtualCall: false)(baseInstance, 456, ref f)); // expected compile error
			Assert.AreEqual("derived test 456 795 3", AccessTools.MethodDelegate<OpenMethodDel<Derived>>(interfaceTest, virtualCall: true)(derivedInstance, 456, ref f));
			_ = Assert.Throws(typeof(ArgumentException), () => AccessTools.MethodDelegate<OpenMethodDel<Derived>>(interfaceTest, virtualCall: false)(derivedInstance, 456, ref f));
			Assert.AreEqual("struct result 456 796 1", AccessTools.MethodDelegate<OpenMethodDel<Struct>>(interfaceTest, virtualCall: true)(structInstance, 456, ref f));
			_ = Assert.Throws(typeof(ArgumentException), () => AccessTools.MethodDelegate<OpenMethodDel<Struct>>(interfaceTest, virtualCall: false)(structInstance, 456, ref f));
		}

		[Test]
		public void Test_AccessTools_MethodDelegate_StaticDelegates_InterfaceMethod()
		{
			var f = 789f;
			Assert.AreEqual("static test 456 790 1", AccessTools.MethodDelegate<MethodDel>(staticTest)(456, ref f));
			// instance and virtualCall args are ignored
			Assert.AreEqual("static test 456 791 2", AccessTools.MethodDelegate<MethodDel>(staticTest, new Base(), virtualCall: false)(456, ref f));
		}

		[Test]
		public void Test_AccessTools_MethodDelegate_InvalidDelegates()
		{
			_ = Assert.Throws(typeof(ArgumentException), () => AccessTools.MethodDelegate<Action>(interfaceTest));
			_ = Assert.Throws(typeof(ArgumentException), () => AccessTools.MethodDelegate<Func<bool>>(baseTest));
			_ = Assert.Throws(typeof(ArgumentException), () => AccessTools.MethodDelegate<Action<string>>(derivedTest));
			_ = Assert.Throws(typeof(ArgumentException), () => AccessTools.MethodDelegate<Func<int, float, string>>(structTest));
		}

		delegate string MethodDel(int n, ref float f);
		delegate string OpenMethodDel<T>(T instance, int n, ref float f);

		[Test]
		public void Test_AccessTools_HarmonyDelegate()
		{
			var someMethod = AccessTools.HarmonyDelegate<AccessToolsHarmonyDelegate.FooSomeMethod>();
			var foo = new AccessToolsHarmonyDelegate.Foo();
			Assert.AreEqual("[test]", someMethod(foo, "test"));
		}
	}
}