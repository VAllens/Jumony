﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ivony.Html;
using Ivony.Fluent;

namespace Ivony.Html.Parser
{


  public class DomFragment : IHtmlFragment, IDomContainer
  {

    public DomFragment( DomFragmentManager manager )
    {
      _manager = manager;
      _nodeCollection = new DomNodeCollection( this );
    }

    public DomFragment( DomFragmentManager manager, string html )
      : this( manager )
    {
      _rawHtml = html;
    }



    protected class FragmentParser : HtmlParserBase
    {
      private DomProvider _provider = new DomProvider();

      protected override IHtmlReader CreateReader( string html )
      {
        return new JumonyReader( html );
      }

      protected override IHtmlDomProvider Provider
      {
        get
        {
          return _provider;
        }
      }

    }

    private object _sync = new object();
    private DomFragmentManager _manager;
    private DomNodeCollection _nodeCollection;
    private string _rawHtml;

    public DomNodeCollection NodeCollection
    {
      get { return _nodeCollection; }
    }


    public IEnumerable<IHtmlNode> Nodes()
    {
      return _nodeCollection.HtmlNodes;
    }

    public object RawObject
    {
      get { return this; }
    }

    public string RawHtml
    {
      get { return null; }
    }

    public IHtmlDocument Document
    {
      get { return _manager.Document; }
    }

    public object SyncRoot
    {
      get { return _sync; }
    }







    public IHtmlElement AddElement( int index, string name, IDictionary<string, string> attributes )
    {
      lock ( SyncRoot )
      {
        var element = new DomElement( name, attributes );
        NodeCollection.Insert( index, element );
        return element;
      }
    }

    public IHtmlTextNode AddTextNode( int index, string htmlText )
    {
      lock ( SyncRoot )
      {
        var textNode = new DomTextNode( htmlText );
        NodeCollection.Insert( index, textNode );
        return textNode;
      }
    }

    public IHtmlComment AddComment( int index, string comment )
    {
      lock ( SyncRoot )
      {
        var commentNode = new DomComment( comment );
        NodeCollection.Insert( index, commentNode );
        return commentNode;
      }
    }

    public IHtmlSpecial AddSpecial( int index, string html )
    {
      throw new NotSupportedException();
    }

    public void Into( IHtmlContainer container, int index )
    {

      if ( container == null )
        throw new ArgumentNullException( "container" );

      if ( !object.Equals( container.Document, Document ) )
        throw new InvalidOperationException();

      var domContainer = container as IDomContainer;
      if ( domContainer == null )
        throw new InvalidOperationException();

      lock ( SyncRoot )
      {

        var nodeList = NodeCollection.ToArray();

        lock ( container.SyncRoot )
        {
          foreach ( var node in nodeList.Reverse() )
          {
            node.Container = null;
            NodeCollection.Remove( node );

            domContainer.NodeCollection.Insert( index, node );
          }
        }


        _manager.Allocated( this );

      }
    }
  }




}


