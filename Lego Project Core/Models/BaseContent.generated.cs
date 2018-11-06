//------------------------------------------------------------------------------
// <auto-generated>
//   This code was generated by a tool.
//
//    Umbraco.ModelsBuilder v3.0.10.102
//
//   Changes to this file will be lost if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Web;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web;
using Umbraco.ModelsBuilder;
using Umbraco.ModelsBuilder.Umbraco;

namespace Lego_Project_Core.Models
{
	// Mixin content Type 1092 with alias "baseContent"
	/// <summary>Base Content</summary>
	public partial interface IBaseContent : IPublishedContent
	{
		/// <summary>Main Content</summary>
		IHtmlString MainContent { get; }

		/// <summary>Title</summary>
		string Title { get; }
	}

	/// <summary>Base Content</summary>
	[PublishedContentModel("baseContent")]
	public partial class BaseContent : PublishedContentModel, IBaseContent
	{
#pragma warning disable 0109 // new is redundant
		public new const string ModelTypeAlias = "baseContent";
		public new const PublishedItemType ModelItemType = PublishedItemType.Content;
#pragma warning restore 0109

		public BaseContent(IPublishedContent content)
			: base(content)
		{ }

#pragma warning disable 0109 // new is redundant
		public new static PublishedContentType GetModelContentType()
		{
			return PublishedContentType.Get(ModelItemType, ModelTypeAlias);
		}
#pragma warning restore 0109

		public static PublishedPropertyType GetModelPropertyType<TValue>(Expression<Func<BaseContent, TValue>> selector)
		{
			return PublishedContentModelUtility.GetModelPropertyType(GetModelContentType(), selector);
		}

		///<summary>
		/// Main Content: Main content for the page
		///</summary>
		[ImplementPropertyType("mainContent")]
		public IHtmlString MainContent
		{
			get { return GetMainContent(this); }
		}

		/// <summary>Static getter for Main Content</summary>
		public static IHtmlString GetMainContent(IBaseContent that) { return that.GetPropertyValue<IHtmlString>("mainContent"); }

		///<summary>
		/// Title: Enter the title
		///</summary>
		[ImplementPropertyType("title")]
		public string Title
		{
			get { return GetTitle(this); }
		}

		/// <summary>Static getter for Title</summary>
		public static string GetTitle(IBaseContent that) { return that.GetPropertyValue<string>("title"); }
	}
}
