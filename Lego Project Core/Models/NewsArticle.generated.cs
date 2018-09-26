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
	/// <summary>News Article</summary>
	[PublishedContentModel("newsArticle")]
	public partial class NewsArticle : PublishedContentModel
	{
#pragma warning disable 0109 // new is redundant
		public new const string ModelTypeAlias = "newsArticle";
		public new const PublishedItemType ModelItemType = PublishedItemType.Content;
#pragma warning restore 0109

		public NewsArticle(IPublishedContent content)
			: base(content)
		{ }

#pragma warning disable 0109 // new is redundant
		public new static PublishedContentType GetModelContentType()
		{
			return PublishedContentType.Get(ModelItemType, ModelTypeAlias);
		}
#pragma warning restore 0109

		public static PublishedPropertyType GetModelPropertyType<TValue>(Expression<Func<NewsArticle, TValue>> selector)
		{
			return PublishedContentModelUtility.GetModelPropertyType(GetModelContentType(), selector);
		}

		///<summary>
		/// Article Content: Primary content for the article
		///</summary>
		[ImplementPropertyType("articleContent")]
		public IHtmlString ArticleContent
		{
			get { return this.GetPropertyValue<IHtmlString>("articleContent"); }
		}

		///<summary>
		/// Headline: Headline for the article
		///</summary>
		[ImplementPropertyType("headline")]
		public string Headline
		{
			get { return this.GetPropertyValue<string>("headline"); }
		}

		///<summary>
		/// Tags: Create tags for this specific news article
		///</summary>
		[ImplementPropertyType("tags")]
		public IEnumerable<string> Tags
		{
			get { return this.GetPropertyValue<IEnumerable<string>>("tags"); }
		}

		///<summary>
		/// Teaser: Small teaser for the article
		///</summary>
		[ImplementPropertyType("teaser")]
		public string Teaser
		{
			get { return this.GetPropertyValue<string>("teaser"); }
		}
	}
}