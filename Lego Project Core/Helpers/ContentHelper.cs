using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Hosting;
using Broen.Core.Cache;
using Broen.Core.Extensions;
using Broen.Core.Models;
using Broen.Core.Resolvers;
using Broen.Core.Services;
using Broen.Core.Viewmodels;
using Nito.AsyncEx.Synchronous;
using Oxygen.Baseline.Core.Helpers;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Web;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;

namespace Broen.Core.Helpers
{
    public class ContentHelper
    {
        /// A new instance Initializes a new instance of the <see cref="ContentHelper"/> class.
        /// with lazy initialization.
        /// </summary>
        private static readonly Lazy<ContentHelper> Lazy = new Lazy<ContentHelper>(() => new ContentHelper());

        /// <summary>
        /// Gets the current instance of the <see cref="ContentHelper"/> class.
        /// </summary>
        public static ContentHelper Instance => Lazy.Value;


        private static BaseSettingsHelper _baseSettingsHelper;

        /// <summary>
        /// The collection of registered types.
        /// </summary>
        private static readonly ConcurrentDictionary<string, Type> RegisteredTypes
            = new ConcurrentDictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// Prevents a default instance of the <see cref="ContentHelper"/> class from being created.
        /// </summary>
        private ContentHelper()
        {

        }

        /// <summary>
        /// Gets or sets the fallback <see cref="UmbracoHelper"/>. 
        /// This is assign during application initialization.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal static UmbracoHelper FallbackUmbracoHelper { get; set; }

        public Stack<MenuItemViewModel> GetBreadCrumb()
        {
            if (IsPageUnderProductPage())
            {
                return GetBreadCrumbForProduct();
            }

            Stack<MenuItemViewModel> pages = new Stack<MenuItemViewModel>();
            var node = UmbracoContext.Current.PublishedContentRequest.PublishedContent;
            IMenu current = node.OfType<IMenu>();
            if (current != null && !current.HideBreadCrumbs)
            {
                while (current != null && current.Level > 0)
                {
                    var parent = current.Parent.OfType<IMenu>();

                    if (parent != null || current.DocumentTypeAlias.Equals(Home.ModelTypeAlias))
                    {
                        var menuItem = new MenuItemViewModel();
                        menuItem.Url = current.Url;
                        menuItem.Name = GetNodeTitle(current);
                        menuItem.Id = current.Id;
                        menuItem.Active = !current.HidePageOnBreadCrumbs;
                        pages.Push(menuItem);
                    }
                    else
                    {
                        break;
                    }

                    current = parent;
                }
            }
            return pages;
        }

        private Stack<MenuItemViewModel> GetBreadCrumbForProduct()
        {
            PublishedContentRequest contentRequest = UmbracoContext.Current.PublishedContentRequest;
            Stack<MenuItemViewModel> pages = null;
            var _breadcrumsCache = new BreadCrumbsCache(new CacheManager());

            System.Threading.Tasks.Task.Factory.StartNew(() => { pages = _breadcrumsCache.GetBreadcrumbsFromCache(contentRequest.Uri.AbsoluteUri).Result; }).WaitWithoutException();

            if (pages != null)
                return pages;

            pages = new Stack<MenuItemViewModel>();
            
            var productPage = GetShopPage(contentRequest.Uri).OfType<ProductsPage>();

            var productService = new ProductService();
            var typeService = new ProductGroupTypeService();
            
            
            if (contentRequest.Uri.Segments.Count() > 5) //Product
            {
                var productId =WebUtility.UrlDecode(contentRequest.Uri.Segments[5].Trim('/'));

                var productDTO = productService.FetchProductById(productId, contentRequest.Culture.Name, ContentHelper.Instance.GetCurrentItemCode());

                if (productDTO != null)
                {
                    pages.Push(new MenuItemViewModel
                    {
                        Url = string.Concat("/", string.Join("", contentRequest.Uri.Segments.Skip(1))),
                        Name = productDTO.ProductTranslation.Current(contentRequest.Culture)?.Description,
                        Active = !productPage.HidePageOnBreadCrumbs
                    });
                }
            }

            if (contentRequest.Uri.Segments.Count() > 4) //Category
            {
                var productCategory = WebUtility.UrlDecode(contentRequest.Uri.Segments[4].Trim('/'));

                var categoryDTO = productService.FetchProductgroupById(productCategory, contentRequest.Culture.Name);

                if (categoryDTO != null)
                {

                    pages.Push(new MenuItemViewModel
                    {
                        Url =
                            string.Concat("/", string.Join("", contentRequest.Uri.Segments.Skip(1).Take(4)))
                                .TrimEnd("/"),
                        Name = categoryDTO.ProductGroupTranslation.Current()?.ProductGroup_Id,
                        Active = !productPage.HidePageOnBreadCrumbs
                    });
                }
            }


            if (contentRequest.Uri.Segments.Count() > 3) //type
            {
                var productType = WebUtility.UrlDecode(contentRequest.Uri.Segments[3].Trim('/'));

                var typeDTO = typeService.FetchProductGroupTypeById(productType, contentRequest.Culture.Name);

                if (typeDTO != null)
                {

                    pages.Push(new MenuItemViewModel
                    {
                        Url =
                            string.Concat("/", string.Join("", contentRequest.Uri.Segments.Skip(1).Take(3)))
                                .TrimEnd("/"),
                        Name = typeDTO.Description,
                        Active = !productPage.HidePageOnBreadCrumbs
                    });
                }
            }


            if (contentRequest.Uri.Segments.Count() > 2) //type
            {
                string urlToFind = string.Join("", contentRequest.Uri.Segments.Take(3));

                var segmentName = urlToFind.Split('/')[2].Replace("-", " ").ToLower();

                var segmentPage = productPage.FirstChild<ProductSegment>(x => x.Name.ToLower() == segmentName).OfType<ProductSegment>();

                if (segmentPage != null && !segmentPage.HideBreadCrumbs)
                {
                    pages.Push(new MenuItemViewModel
                    {
                        Url = segmentPage.Url,
                        Name = segmentPage.Description,
                        Id = segmentPage.Id,
                        Active = !segmentPage.HidePageOnBreadCrumbs
                    });
                }

            }


            if (productPage != null && !productPage.HideBreadCrumbs)
            {
                pages.Push(new MenuItemViewModel
                {
                    Url = productPage.Url,
                    Name = GetNodeTitle(productPage),
                    Id = productPage.Id,
                    Active = !productPage.HidePageOnBreadCrumbs
                });
            }

            pages.Push(new MenuItemViewModel()
            {
                Url = "/",
                Name = "Home",
                Active = true
            });

            System.Threading.Tasks.Task.Factory.StartNew(() => { _breadcrumsCache.InsertBreadcrumbIntoCache(contentRequest.Uri.AbsoluteUri, pages); }).WaitWithoutException();


            return pages;
        }

        private bool IsPageUnderProductPage()
        {
            PublishedContentRequest contentRequest = UmbracoContext.Current.PublishedContentRequest;

            var productPage = ContentHelper.Instance.GetShopPage(contentRequest.Uri);
            if (productPage == null)
            {
                LogHelper.Warn<ProductResolver>("Productpage missing");
                return false;
            }
            IPublishedContent node = null;
            var uri = new Uri(productPage.UrlAbsolute());
            var shopPageUrl = string.Concat("/", string.Join("", uri.Segments.Skip(1))); //Skip protocol

            return contentRequest.Uri.AbsoluteUri.Contains(shopPageUrl.TrimEnd('/'));
        }

        public IPublishedContent GetHomePage()
        {
            return GetRootContent().FirstOrDefault(x => x.DocumentTypeAlias == Home.ModelTypeAlias);
        }

        public IPublishedContent GetCurrentHomePage(Uri uri)
        {
            var pageId = ContentHelper.Instance.UmbracoHelper.UmbracoContext.PageId;

            if (pageId == null)
            {
                var id = ApplicationContext.Current.Services.DomainService.GetByName(uri.Host).RootContentId;

                if (id != null) return GetSiteRootContent(id.Value);
            }

            return GetSiteRootContent(pageId.Value);
        }

        public string GetCurrentItemCode(string fallbackValue = "broenuk")
        {
            var home = GetHomePage().OfType<Home>();
            var itemCode = fallbackValue;

            if (home != null)
            {
                itemCode = home.ItemCode;
            }

            return itemCode.ToLower().Trim().Replace("_", "").Replace(" ", "");
        }

        public string GetCurrentItemCode(Uri uri, string fallbackValue = "broenuk")
        {
            var home = GetCurrentHomePage(uri).OfType<Home>();
            var itemCode = fallbackValue;

            if (home != null)
            {
                itemCode = home.ItemCode;
            }

            return itemCode.ToLower().Trim().Replace("_", "").Replace(" ", "");
        }

        public string GetCurrentItemCode(int modelId, string fallbackValue = "broenuk")
        {
            var home = GetSiteRootContent(modelId).FirstChild().OfType<Home>();
            var itemCode = fallbackValue;

            if (home != null)
            {
                itemCode = home.ItemCode;
            }

            return itemCode.ToLower().Trim().Replace("_", "").Replace(" ", "");
        }
        public string GetCurrentSegment(Uri uri)
        {
            if (uri == null)
                return "";

            var productPage = ContentHelper.Instance.GetShopPage(uri);
            if (productPage == null)
            {
                return "";
            }

            var uriShop = new Uri(productPage.UrlAbsolute());
            var shopPageUrl = string.Concat("/", string.Join("", uriShop.Segments.Skip(1))).TrimEnd('/'); //Skip protocol

            //Url are formatet as: /product-segment/product-category/product-name
            if (uri.AbsoluteUri.Contains(shopPageUrl))


                if (uri.Segments.Count() > 2) //We have a product group type
                {
                    var segmentFromUrl = uri.Segments[2].Trim('/').ToLower();

                    var temp = string.Join("", uri.Segments.Take(3));
                    var segmentNode = UmbracoContext.Current.ContentCache.GetByRoute(temp);

                    if (segmentNode != null)
                    {

                        var segment = segmentNode.OfType<ProductSegment>();

                        if (segment != null)
                        {
                            return segment.Code.ToLower();
                        } 

                    }

                    return segmentFromUrl;
                }

            return "";
        }

        public string GetCurrentType(Uri uri)
        {
            if (uri == null)
                return "";

            var productPage = ContentHelper.Instance.GetShopPage(uri);
            if (productPage == null)
            {
                return "";
            }

            var uriShop = new Uri(productPage.UrlAbsolute());
            var shopPageUrl = string.Concat("/", string.Join("", uriShop.Segments.Skip(1))).TrimEnd('/'); //Skip protocol

            //Url are formatet as: /product-segment/product-category/product-name
            if (uri.AbsoluteUri.Contains(shopPageUrl))


                if (uri.Segments.Count() > 3) //We have a product group type
                {
                    return uri.Segments[3].Trim().Trim('/').ToLower();
                }


            return "";
        }

        public static string GetNodeTitle(IPublishedContent publishedContent)
        {
            var menu = publishedContent.OfType<IMenu>();
            if (menu != null)
            {
                if (!string.IsNullOrEmpty(menu.Title))
                    return menu.Title;
            }

            var seo = publishedContent.OfType<ISEO>();
            if (seo != null)
            {
                if (!string.IsNullOrEmpty(seo.PageTitle))
                    return seo.PageTitle;
            }

            return publishedContent.Name;
        }

        public static string GetPageTitle(IPublishedContent publishedContent)
        {
            var productPage = publishedContent.OfType<ProductViewPage>();
            if (productPage != null)
            {
                if (!string.IsNullOrEmpty(productPage.PageTitle))
                {
                    return string.Format(productPage.PageTitle, productPage.ProductName);
                }
            }

            var productgroup = publishedContent.OfType<ProductGroup>();
            if (productgroup != null)
            {
                if (!string.IsNullOrEmpty(productgroup.PageTitle))
                {
                    return string.Format(productgroup.PageTitle, productgroup.GroupName);
                }
            }


            var seo = publishedContent.OfType<ISEO>();
            if (seo != null)
            {
                if (!string.IsNullOrEmpty(seo.PageTitle))
                    return seo.PageTitle;
            }

            var menu = publishedContent.OfType<IMenu>();
            if (menu != null)
            {
                if (!string.IsNullOrEmpty(menu.Title))
                    return menu.Title;
            }

            

            return publishedContent.Name;
        }

        /// <summary>
        /// Gets the <see cref="UmbracoHelper"/> for querying published content or media.
        /// </summary>
        public UmbracoHelper UmbracoHelper
        {
            get
            {
                if (HttpContext.Current != null)
                {
                    // Pull the item from the cache if possible to reduce the overhead for multiple operations
                    // taking place in a single request
                    return (UmbracoHelper)ApplicationContext.Current.ApplicationCache.RequestCache.GetCacheItem(
                        "ContentHelper.Instance.UmbracoHelper",
                        () =>
                        {
                            //HttpContextBase context = new HttpContextWrapper(HttpContext.Current);
                            var dummyHttpContext = new HttpContextWrapper(new HttpContext(new SimpleWorkerRequest("blah.aspx", "", new StringWriter())));
                            ApplicationContext application = ApplicationContext.Current;
                            WebSecurity security = new WebSecurity(dummyHttpContext, application);
                            IUmbracoSettingsSection umbracoSettings = UmbracoConfig.For.UmbracoSettings();
                            IEnumerable<IUrlProvider> providers = UrlProviderResolver.Current.Providers;
                            return new UmbracoHelper(UmbracoContext.EnsureContext(dummyHttpContext, application, security, umbracoSettings, providers, false));
                        });
                }

                return FallbackUmbracoHelper;
            }
        }



        public GlobalSettingsFolder GetSettings()
        {
            GlobalSettingsFolder siteSettings = GetRootContent()?.FirstOrDefault(x => x.DocumentTypeAlias == GlobalSettingsFolder.ModelTypeAlias)?.OfType<GlobalSettingsFolder>();
            return siteSettings;
        }

        public string GetGoogleMapsKey()
        {
            return GetSettings().APikey;
        }

        public string GetGoogleMapsInitUrl(string initMethod)
        {
            return "https://maps.googleapis.com/maps/api/js?key=" + GetGoogleMapsKey() +
                   "&callback=" + initMethod;
        }

        public IEnumerable<IPublishedContent> GetRootContent()
        {
            return UmbracoHelper.TypedContentAtRoot();
        }

        public LocalSetting GetLocalSetting()
        {
            var baseSettingHelper = new BaseSettingsHelper();

            var siteSetting = baseSettingHelper.GetPageSettingsRootNode();
          
            return siteSetting.OfType<LocalSetting>();
        }


        public LocalSetting GetLocalSetting(int currentNodeId)
        {
            var setting = GetSiteRootContent(currentNodeId).FirstChild(LocalSetting.ModelTypeAlias);

            if (setting == null)
                LogHelper.Warn<ContentHelper>($"LocalSetting node was null. Was trying to find localSetting node for the site that has node ID: {currentNodeId}");

            return setting.OfType<LocalSetting>();
        }

        public IPublishedContent GetSiteRootContent(int currentNodeId)
        {
            var currentNode = UmbracoHelper.TypedContent(currentNodeId);
            if (currentNode == null)
                return null;

            var rootOfTheCurrentSite = currentNode.Path.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Where(x => !x.Equals("-1")).FirstOrDefault();

            return UmbracoHelper.TypedContent(int.Parse(rootOfTheCurrentSite));
        }

        public IPublishedContent GetShopPage(Uri uri)
        {
            var rootNode = GetCurrentHomePage(uri);
            return rootNode.Descendant(ProductsPage.ModelTypeAlias);
        }



        public ProductType GetProductGroupType (int productNode)
        {
            var localSetting = GetLocalSetting(productNode);

            var type = localSetting.FirstChild<ProductType>();

            if (type == null)
            {
                LogHelper.Warn<ContentHelper>($"Could not get ProductType. Value was null");
                return null;
            }

            return type.OfType<ProductType>();
            
        }

        public ProductViewPage GetProduct (int productNode)
        {

            var localSetting = GetLocalSetting(productNode);

            var product = localSetting.FirstChild<ProductViewPage>();

            if (product == null)
                return null;

            return product.OfType<ProductViewPage>();
            
        }

        public ProductGroup GetProductGroup(int productNode)
        {

            var localSetting = GetLocalSetting(productNode);

            var product = localSetting.FirstChild<ProductGroup>();

            if (product == null)
                return null;

            return product.OfType<ProductGroup>();
            
        }

        public IEnumerable<string> GetDropDownPrevaluesByDataTypeName(string name)
        {
            var dataType = UmbracoHelper.DataTypeService.GetAllDataTypeDefinitions()
                .FirstOrDefault(x => x.Name == name);

            if(dataType == null)
                return new List<string>();

            var id = dataType.Id;

            return UmbracoHelper.DataTypeService.GetPreValuesByDataTypeId(id);
        }
    }
}
