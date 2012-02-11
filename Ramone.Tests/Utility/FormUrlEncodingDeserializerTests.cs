﻿using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Ramone.Utility;
using Ramone.Utility.ObjectSerialization;


namespace Ramone.Tests.Utility
{
  [TestFixture]
  public class FormUrlEncodingDeserializerTests : TestHelper
  {
    [Test]
    public void CanDeserializeSimpleTypes()
    {
      // Arrange
      string s = "MyInt=10&MyString=Abc";

      // Act
      SimpleData data = Deserialize<SimpleData>(s);

      // Assert
      Assert.IsNotNull(data);
      Assert.AreEqual(10, data.MyInt);
      Assert.AreEqual("Abc", data.MyString);
    }


    [Test]
    public void CanDeserializeEmptyValues()
    {
      // Arrange
      string s = "MyInt=&MyString=";

      // Act
      SimpleData data = Deserialize<SimpleData>(s);

      // Assert
      Assert.IsNotNull(data);
      Assert.AreEqual(0, data.MyInt);
      Assert.AreEqual("", data.MyString);
    }


    [Test]
    public void CanDeserializeEmptyInput()
    {
      // Arrange
      string s = "";

      // Act
      SimpleData data = Deserialize<SimpleData>(s);

      // Assert
      Assert.IsNotNull(data);
      Assert.AreEqual(0, data.MyInt);
      Assert.AreEqual(null, data.MyString);
    }


    [Test]
    public void CanDeserializeMissingAssignments()
    {
      // Arrange
      string s = "MyInt&MyString";

      // Act
      SimpleData data = Deserialize<SimpleData>(s);

      // Assert
      Assert.IsNotNull(data);
      Assert.AreEqual(0, data.MyInt);
      Assert.AreEqual(null, data.MyString);
    }


    [Test]
    public void WhenDeserializingItIgnoresExtraValues()
    {
      // Arrange
      string s = "Z=1&MyInt=2&MyString=Xyz&Y=2";

      // Act
      SimpleData data = Deserialize<SimpleData>(s);

      // Assert
      Assert.IsNotNull(data);
      Assert.AreEqual(2, data.MyInt);
      Assert.AreEqual("Xyz", data.MyString);
    }


    [Test]
    public void CanDeserializeNestedTypes()
    {
      // Arrange
      string s = "MyInt=555&Simple.MyInt=10&Simple.MyString=Abc";

      // Act
      NestedData data = Deserialize<NestedData>(s);

      // Assert
      Assert.IsNotNull(data);
      Assert.AreEqual(555, data.MyInt);
      Assert.IsNotNull(data.Simple);
      Assert.AreEqual("Abc", data.Simple.MyString);
      Assert.AreEqual(10, data.Simple.MyInt);
    }


    protected T Deserialize<T>(string s)
      where T : class
    {
      FormUrlEncodingSerializer serializer = new FormUrlEncodingSerializer(typeof(T));

      using (TextReader reader = new StringReader(s))
      {
        T data = (T)serializer.Deserialize(reader);
        return data;
      }
    }


    public class SimpleData
    {
      public int MyInt { get; set; }
      public string MyString { get; set; }
      //public DateTime MyDate { get; set; }
    }


    public class NestedData
    {
      public int MyInt { get; set; }
      public SimpleData Simple { get; set; }
    }


    public class DictionaryData
    {
      public int MyInt { get; set; }
      public Dictionary<string, object> MyDict { get; set; }
      //public DateTime MyDate { get; set; }
    }
  }
}