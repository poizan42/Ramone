﻿using System.Web;
using OpenRasta.Web;
using Ramone.MediaTypes.Atom;
using Ramone.Tests.Common.CMS;
using System;


namespace Ramone.Tests.Server.CMS.Handlers
{
  public class DossiersHandler
  {
    public ICommunicationContext Context { get; set; }


    public Dossier Get(long id)
    {
      Party party = new PartyHandler().Get(19);

      return new Dossier
      {
        Id = id,
        Title = string.Format("Dossier no. {0}", id),
        Links = new AtomLinkList
        {
          //new AtomLink(typeof(DossierDocumentList).CreateUri(new { id = id }), CMSConstants.DocumentsLinkRelType, CMSConstants.CMSMediaTypeId, "Documents"),
          new AtomLink("documents", CMSConstants.DocumentsLinkRelType, CMSConstants.CMSMediaTypeId, "Documents"),
          new AtomLink(party.CreateUri(), CMSConstants.PartyLinkRelType, CMSConstants.CMSMediaTypeId, party.FullName)
        }
      };
    }


    public OperationResult Post(Dossier dossier)
    {
      Dossier d = new Dossier
      {
        Id = 999,
        Title = dossier != null ? dossier.Title : null
      };

      if (dossier != null && dossier.Title == "Do not return body")
        d = null;

      return new OperationResult.Created
      {
        ResponseResource = d,
        RedirectLocation = typeof(Dossier).CreateUri(new { id = 999 })
      };
    }


    public OperationResult Post(string method, Dossier dossier)
    {
      if (method != "POST")
        throw new InvalidOperationException(string.Format("Unexpected method (should have been {0}, was POST'.", method));
      return Post(dossier);
    }


    public OperationResult Post(string method)
    {
      if (method != "POST")
        throw new InvalidOperationException(string.Format("Unexpected method (should have been {0}, was POST'.", method));
      return Post((Dossier)null);
    }
    
    
    public OperationResult Put(Dossier dossier)
    {
      return new OperationResult.Created
      {
        ResponseResource = dossier,
        RedirectLocation = dossier.CreateUri()
      };
    }


    public object Patch(long id, string title = null)
    {
      return (title ?? "<null>") + ": ok";
    }


    public OperationResult Delete(long id)
    {
      return new OperationResult.OK("Deleted, yup!");
    }


    public object Head(long id)
    {
      HttpContext.Current.Response.Headers["X-ExtraHeader"] = "1";
      return null;
    }


    public object Options(long id)
    {
      HttpContext.Current.Response.Headers["X-ExtraHeader"] = "2";

      return "Yes";
   }
  }
}