﻿using System.IO;
using System.Linq;
using NUnit.Framework;
using Ramone.Tests.Common;

namespace Ramone.Tests.MediaTypes
{
  [TestFixture]
  public class StreamCodecTests : TestHelper
  {
    [Test]
    public void CanGetStream()
    {
      // Arrange
      Request fileReq = Session.Bind(FileTemplate);

      // Act
      using (Response<Stream> response = fileReq.Accept("application/octet-stream").Get<Stream>())
      {
        // Assert
        Assert.AreEqual(12, response.ContentLength);
        byte[] data = new byte[12];
        response.Body.Read(data, 0, 12);
        Assert.AreEqual((int)'H', data[0]);
        Assert.AreEqual((int)'e', data[1]);
        Assert.AreEqual((int)'l', data[2]);
        Assert.AreEqual((int)'l', data[3]);
      }
    }

    
    [Test]
    public void CanPostStream()
    {
      // Arrange
      Request fileReq = Session.Bind(FileTemplate);

      // Act
      using (MemoryStream s = new MemoryStream(new byte[] { 10,2,30,4 }))
      {
        using (Response<Stream> response =
          fileReq.Accept("application/octet-stream").ContentType("application/octet-stream").Post<Stream>(s))
        {
          // Assert
          Assert.AreEqual(4, response.ContentLength);
          Assert.AreEqual(10, response.Body.ReadByte());
          Assert.AreEqual(2, response.Body.ReadByte());
          Assert.AreEqual(30, response.Body.ReadByte());
          Assert.AreEqual(4, response.Body.ReadByte());
        }
      }
    }


    [Test]
    public void CanPostStreamWithDefaultMediaTypeOctetStream()
    {
      // Arrange
      Request fileReq = Session.Bind(Constants.HeaderEchoPath);

      // Act
      using (MemoryStream s = new MemoryStream(new byte[] { 10, 2, 30, 4 }))
      {
        using (Response<HeaderList> response = fileReq.Post<HeaderList>(s))
        {
          HeaderList headers = response.Body;

          // Assert
          Assert.IsNotNull(headers);
          Assert.IsTrue(headers.Any(h => h == "Content-Type: application/octet-stream"), "Must contain content type octet stream");
        }
      }
    }
  }
}
