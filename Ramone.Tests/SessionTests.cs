﻿using System;
using System.Globalization;
using System.Linq;
using System.Net.Cache;
using System.Text;
using NUnit.Framework;
using Ramone.Utility.ObjectSerialization;
using Ramone.Tests.Common.CMS;
using Ramone.MediaTypes.Xml;


namespace Ramone.Tests
{
  [TestFixture]
  public class SessionTests : TestHelper
  {
    IService MySettings = RamoneConfiguration.NewService(BaseUrl);


    [Test]
    public void WhenCreatingSessionItCopiesAllSimpleProperties()
    {
      // Arrange
      HttpRequestCachePolicy policy = new HttpRequestCachePolicy(HttpRequestCacheLevel.CacheOrNextCacheOnly);

      IService service = RamoneConfiguration.NewService(BaseUrl);
      service.UserAgent = "Dummy";
      service.DefaultEncoding = Encoding.ASCII;
      service.DefaultRequestMediaType = new MediaType("X/1");
      service.DefaultResponseMediaType = new MediaType("Y/1");
      service.CachePolicy = policy;

      // Act
      ISession session = service.NewSession();

      // Assert
      Assert.AreEqual("Dummy", session.UserAgent);
      Assert.AreEqual(Encoding.ASCII, session.DefaultEncoding);
      Assert.AreEqual(new MediaType("X/1"), session.DefaultRequestMediaType);
      Assert.AreEqual(new MediaType("Y/1"), session.DefaultResponseMediaType);
      Assert.AreEqual(BaseUrl, session.BaseUri);
      Assert.AreEqual(policy, session.CachePolicy);
    }


    [Test]
    public void WhenCreatingSessionItClonesAuthorizationDispatcher()
    {
      // Arrange
      IService service = RamoneConfiguration.NewService(BaseUrl);

      // Act
      ISession session1 = service.NewSession();

      service.AuthorizationDispatcher.Add("dummy1", new DummyHandler1());
      
      ISession session2 = service.NewSession();
      session2.AuthorizationDispatcher.Add("dummy2", new DummyHandler2());

      ISession session3 = service.NewSession();

      // Assert
      Assert.IsNull(session1.AuthorizationDispatcher.Get("dummy1"));
      Assert.IsNull(session1.AuthorizationDispatcher.Get("dummy2"));
      
      Assert.IsNotNull(session2.AuthorizationDispatcher.Get("dummy1"));
      Assert.IsNotNull(session2.AuthorizationDispatcher.Get("dummy2"));
      
      Assert.IsNotNull(session3.AuthorizationDispatcher.Get("dummy1"));
      Assert.IsNull(session3.AuthorizationDispatcher.Get("dummy2"));
    }


    [Test]
    public void WhenCreatingSessionItClonesRedirectSettings()
    {
      // Arrange
      IService service = RamoneConfiguration.NewService(BaseUrl);

      // Act
      ISession session1 = service.NewSession();

      service.SetAllowedRedirects(300, 11);

      ISession session2 = service.NewSession();
      session2.SetAllowedRedirects(301, 7);

      ISession session3 = service.NewSession();

      // Assert
      Assert.AreEqual(11, service.GetAllowedRedirects(300));
      Assert.AreEqual(0, session1.GetAllowedRedirects(300));
      Assert.AreEqual(11, session2.GetAllowedRedirects(300));
      Assert.AreEqual(7, session2.GetAllowedRedirects(301));
    }


    [Test]
    public void WhenCreatingSessionItClonesInterceptors()
    {
      // Arrange
      IService service = RamoneConfiguration.NewService(BaseUrl);

      // Act
      ISession session1 = service.NewSession();

      service.RequestInterceptors.Add(new DummyInterceptor1());

      ISession session2 = service.NewSession();
      session2.RequestInterceptors.Add(new DummyInterceptor2());

      ISession session3 = service.NewSession();

      // Assert
      Assert.AreEqual(0, session1.RequestInterceptors.Count());

      Assert.AreEqual(2, session2.RequestInterceptors.Count());
      
      Assert.AreEqual(1, session3.RequestInterceptors.Count());
    }


    [Test]
    public void WhenCreatingSessionItClonesSerializerSettings()
    {
      // Arrange
      IService service = RamoneConfiguration.NewService(BaseUrl);
      service.SerializerSettings.ArrayFormat = "A";
      service.SerializerSettings.DictionaryFormat = "B";
      service.SerializerSettings.PropertyFormat = "C";
      service.SerializerSettings.DateTimeFormat = "O";
      service.SerializerSettings.Formaters.AddFormater(typeof(SomeClass1), new SomeClass1Formater());
      service.SerializerSettings.Culture = CultureInfo.GetCultureInfo("da-DK");
      service.SerializerSettings.Encoding = Encoding.ASCII;
      service.SerializerSettings.EnableNonAsciiCharactersInMultipartFilenames = true;

      // Act 1
      ISession session = service.NewSession();

      // Assert
      Assert.AreEqual("A", session.SerializerSettings.ArrayFormat);
      Assert.AreEqual("B", session.SerializerSettings.DictionaryFormat);
      Assert.AreEqual("C", session.SerializerSettings.PropertyFormat);
      Assert.AreEqual("O", session.SerializerSettings.DateTimeFormat);
      Assert.AreEqual("da-DK", session.SerializerSettings.Culture.Name);
      Assert.AreEqual(Encoding.ASCII, session.SerializerSettings.Encoding);
      Assert.IsTrue(session.SerializerSettings.EnableNonAsciiCharactersInMultipartFilenames);
      
      // Act 2
      session.SerializerSettings.ArrayFormat = "A2";
      session.SerializerSettings.DictionaryFormat = "B2";
      session.SerializerSettings.PropertyFormat = "C2";
      session.SerializerSettings.DateTimeFormat = "R";
      session.SerializerSettings.Formaters.AddFormater(typeof(SomeClass2), new SomeClass2Formater());
      session.SerializerSettings.Culture = CultureInfo.GetCultureInfo("pt-BR");
      session.SerializerSettings.Encoding = Encoding.Unicode;
      session.SerializerSettings.EnableNonAsciiCharactersInMultipartFilenames = false;

      // Assert
      Assert.AreEqual("A2", session.SerializerSettings.ArrayFormat);
      Assert.AreEqual("B2", session.SerializerSettings.DictionaryFormat);
      Assert.AreEqual("C2", session.SerializerSettings.PropertyFormat);
      Assert.AreEqual("R", session.SerializerSettings.DateTimeFormat);
      Assert.AreEqual("pt-BR", session.SerializerSettings.Culture.Name);
      Assert.AreEqual(Encoding.Unicode, session.SerializerSettings.Encoding);
      Assert.IsFalse(session.SerializerSettings.EnableNonAsciiCharactersInMultipartFilenames);
      Assert.AreEqual("A", service.SerializerSettings.ArrayFormat);
      Assert.AreEqual("B", service.SerializerSettings.DictionaryFormat);
      Assert.AreEqual("C", service.SerializerSettings.PropertyFormat);
      Assert.AreEqual("O", service.SerializerSettings.DateTimeFormat);
      Assert.AreEqual("da-DK", service.SerializerSettings.Culture.Name);
      Assert.AreEqual(Encoding.ASCII, service.SerializerSettings.Encoding);
      Assert.IsTrue(service.SerializerSettings.EnableNonAsciiCharactersInMultipartFilenames);
      Assert.IsNotNull(session.SerializerSettings.Formaters.GetFormater(typeof(SomeClass1)));
      Assert.IsNotNull(session.SerializerSettings.Formaters.GetFormater(typeof(SomeClass2)));
      Assert.IsNotNull(service.SerializerSettings.Formaters.GetFormater(typeof(SomeClass1)));
      Assert.IsNull(service.SerializerSettings.Formaters.GetFormater(typeof(SomeClass2)));
    }


    [Test]
    public void CanSkipUseOfService()
    {
      // Act
      ISession session = RamoneConfiguration.NewSession(BaseUrl);

      // Assert
      Assert.IsNotNull(session.Service);
      Assert.IsNotNull(session.Service.BaseUri);
      Assert.AreEqual(BaseUrl, BaseUrl.AbsoluteUri);
    }


    [Test]
    public void CanSetAndGetSessionItems()
    {
      // Arrange
      ISession session = RamoneConfiguration.NewSession(BaseUrl);

      // Act
      session.Items["X"] = 1234;
      int x = (int)session.Items["X"];

      // Assert
      Assert.AreEqual(1234, x);
    }


    [Test]
    public void CanCreateSessionWithoutBaseUrlAndMakeAbsoluteRequests()
    {
      // Act
      ISession session = RamoneConfiguration.NewSession();

      session.Service.CodecManager.AddCodec<Dossier, XmlSerializerCodec>(CMSConstants.CMSMediaType);
      Request req = session.Bind(new Uri(BaseUrl, CMSConstants.DossierPath.Replace("{id}", "0")));
      using (var resp = req.Get<Dossier>())
      {
        // Assert
        Assert.IsNotNull(resp.Body);
        Assert.AreEqual(0, resp.Body.Id);
      }
    }


    [Test]
    public void WhenNotUsingBaseUrlItThrowsInvalidOperationException()
    {
      // Act
      ISession session = RamoneConfiguration.NewSession();

      AssertThrows<InvalidOperationException>(
        () => session.Bind(DossierTemplate, new { id = 2 }),
        ex => ex.Message.Contains("base URL"));
    }


    class DummyHandler1 : IAuthorizationHandler
    {
      public bool HandleAuthorizationRequest(AuthorizationContext context)
      {
        return true;
      }
    }


    class DummyHandler2 : IAuthorizationHandler
    {
      public bool HandleAuthorizationRequest(AuthorizationContext context)
      {
        return true;
      }
    }


    class DummyInterceptor1 : IRequestInterceptor
    {
      public void HeadersReady(RequestContext context)
      {
      }

      public void DataSent(RequestContext context)
      {
      }
    }


    class DummyInterceptor2 : IRequestInterceptor
    {
      public void HeadersReady(RequestContext context)
      {
      }

      public void DataSent(RequestContext context)
      {
      }
    }


    class SomeClass1
    {
    }


    public class SomeClass1Formater : IObjectSerializerFormater
    {
      #region IObjectSerializerFormater Members

      public string Format(object src)
      {
        throw new NotImplementedException();
      }

      #endregion
    }


    class SomeClass2
    {
    }


    public class SomeClass2Formater : IObjectSerializerFormater
    {
      #region IObjectSerializerFormater Members

      public string Format(object src)
      {
        throw new NotImplementedException();
      }

      #endregion
    }
  }
}
