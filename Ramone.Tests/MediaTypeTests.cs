﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;


namespace Ramone.Tests
{
  [TestFixture]
  public class MediaTypeTests : TestHelper
  {
    [Test]
    public void CanConstructMediaTypeFromString()
    {
      // Act
      MediaType m = new MediaType("text/plain");

      // Assert
      Assert.AreEqual("text/plain", (string)m);
      Assert.AreEqual("text", m.TopLevelType);
      Assert.AreEqual("plain", m.SubType);
    }


    [Test]
    public void ItOnlyReadsMediaTypeAndThrowsAwayOtherParameters()
    {
      // Act
      MediaType m = new MediaType("text/plain; charset=utf-8");

      // Assert
      Assert.AreEqual("text/plain", (string)m);
      Assert.AreEqual("text", m.TopLevelType);
      Assert.AreEqual("plain", m.SubType);
    }


    [Test]
    public void CanConstructWildcards()
    {
      // Act
      MediaType m1 = new MediaType("text/*");
      MediaType m2 = new MediaType("*/*");

      // Assert
      Assert.AreEqual("text/*", (string)m1);
      Assert.AreEqual("text", m1.TopLevelType);
      Assert.AreEqual("*", m1.SubType);
      Assert.IsFalse(m1.IsTopLevelWildcard);
      Assert.IsTrue(m1.IsSubTypeWildcard);
      Assert.IsFalse(m1.IsWildcard);

      Assert.AreEqual("*/*", (string)m2);
      Assert.AreEqual("*", m2.TopLevelType);
      Assert.AreEqual("*", m2.SubType);
      Assert.IsTrue(m2.IsTopLevelWildcard);
      Assert.IsTrue(m2.IsSubTypeWildcard);
      Assert.IsTrue(m2.IsWildcard);
    }


    [Test]
    public void CanMatchWildcards()
    {
      // Arrange
      MediaType m1 = new MediaType("text/plain");
      MediaType m2 = new MediaType("text/*");
      MediaType m3 = new MediaType("*/*");

      // Assert
      Assert.IsTrue(m1.Matches("text/plain"));
      Assert.IsTrue(m1.Matches("text/PLAIN"));
      Assert.IsFalse(m1.Matches("text/html"));
      Assert.IsFalse(m1.Matches("image/html"));
      Assert.IsFalse(m1.Matches("imAGe/hTML"));
      Assert.IsTrue(m2.Matches("text/plain"));
      Assert.IsTrue(m2.Matches("TEXT/html"));
      Assert.IsFalse(m2.Matches("image/html"));
      Assert.IsTrue(m3.Matches("text/plain"));
      Assert.IsTrue(m3.Matches("text/HTML"));
      Assert.IsTrue(m3.Matches("image/html"));
    }


    [Test]
    public void ThrowsOnInvalidMediaTypes()
    {
      AssertThrows<ArgumentNullException>(() => new MediaType((string)null));
      AssertThrows<FormatException>(() => new MediaType(""));
      AssertThrows<FormatException>(() => new MediaType("text"));
      AssertThrows<FormatException>(() => new MediaType("text/"));
      AssertThrows<FormatException>(() => new MediaType("text/"));
      AssertThrows<FormatException>(() => new MediaType("text/xxx/qqq"));
      AssertThrows<FormatException>(() => new MediaType("  ; charset=utf-8"));
    }


    [Test]
    public void ItComparesCaseInsensitive()
    {
      // Arrange
      MediaType m1 = new MediaType("x/y");
      MediaType m2 = new MediaType("X/Y");

      // Assert
      Assert.IsTrue(m1 == m2);
    }


    [Test]
    public void CanAssignMediaTypeFromString()
    {
      // Act
      MediaType m = "app/x";

      // Assert
      Assert.AreEqual("app/x", (string)m);
    }


    [Test]
    public void CanExplicitlyCastMediaTypeToString()
    {
      // Ararnge
      MediaType mt = "app/x";

      // Act
      string m = (string)mt;

      // Assert
      Assert.AreEqual("app/x", m);
    }


    [Test]
    public void CanExplicitlyCastNullMediaTypeToString()
    {
      // Ararnge
      MediaType mt = null;

      // Act
      string m = (string)mt;

      // Assert
      Assert.IsNull(m);
    }


    [Test]
    public void WhenCreatingMediaTypeFromNullValueItReturnsValue()
    {
      // Act
      MediaType m = MediaType.Create(null);

      // Assert
      Assert.IsNull(m);
    }


    [Test]
    public void CanCompareEmptyStringWithMediaType()
    {
      string ct = "";
      Assert.IsFalse(ct == MediaType.ApplicationXml);
      Assert.IsFalse(MediaType.ApplicationXml == ct);
      Assert.IsTrue(ct != MediaType.ApplicationXml);
      Assert.IsTrue(MediaType.ApplicationXml != ct);
    }
  }
}
