using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IjepaiMailer
{
    public class MailTemplate
    {

        #region Local Variables
     //   private MailTemplateEntity _mailTemplateEntity;
      //  private IDataRepository<MailTemplateEntity> _securityTableService;
      //  private EntityDataModel _dataContext;

        #endregion

        #region Properties

        public string Title
        {
            get { return Title; }
            set { Title = value; }
        }
        public string Description
        {
            get { return Description; }
            set { Description = value; }
        }
        public string Subject
        {
            get { return Subject; }
            set { Subject = value; }
        }

        public string BodyContent
        {
            get { return BodyContent; }
            set { BodyContent = value; }
        }
        #endregion

        #region Constructor

        public MailTemplate()
        {
            //SetDataEntity();
        }

        //public MailTemplate(string orgUnitId, string code)
        //{
        //    SetDataEntity();
        //    LoadMailTemplate(orgUnitId, code);
        //}
        #endregion

        //public void SetDataEntity()
        //{
        //    _securilTemplateEntity = new MailTemplateEntity();
        //}

        //public void LoadMailTemplate(string orgUnitId, string code)
        //{
        //    var results = from r in _dataContext.MailTemplateEntity
        //                  where 1 == 1 && r.PartitionKey == orgUnitId && r.RowKey == code
        //                  select r;
        //    MailTemplateEntity mailTemplateEntity;
        //    try
        //    {
        //        mailTemplateEntity = results.FirstOrDefault<MailTemplateEntity>();
        //    }
        //    catch (Exception e)
        //    {
        //        mailTemplateEntity = null;
        //        Log.Write(EventKind.Error, "Error loading for the BatchActionItem server reported: " + e.Message);
        //    }

        //    if (mailTemplateEntity == null)
        //    {
        //        mailTemplateEntity = new MailTemplateEntity();
        //        mailTemplateEntity.OrgUnitId = Guid.Parse(orgUnitId);
        //        mailTemplateEntity.Code = code;
        //    }
        //    Load(mailTemplateEntity);

        //}

        //public void Load(MailTemplateEntity mailTemplateEntity)
        //{
        //    _mailTemplateEntity = mailTemplateEntity;

        //}

        //public void Add()
        //{
        //    try
        //    {
        //        _securityTableService.AddEntity(_mailTemplateEntity);
        //        _securityTableService.SaveChanges();

        //    }
        //    catch (Exception e)
        //    {
        //        string exception = string.Format("{0}\n {1}", e.Message, e.StackTrace);
        //        Log.Write(EventKind.Error, "Error saving new MailTemplate Error Message:" + exception);
        //    }
        //}

        //public void Update()
        //{
        //    try
        //    {
        //        if (_mailTemplateEntity.PartitionKey == _mailTemplateEntity.OrgUnitId.ToString() &&
        //            _mailTemplateEntity.RowKey == _mailTemplateEntity.Code.ToString())
        //        {
        //            _securityTableService.UpdateEntity(_mailTemplateEntity);
        //            _securityTableService.SaveChanges();
        //        }
        //        else
        //        {
        //            _securityTableService.DeleteEntity(_mailTemplateEntity);
        //            _securityTableService.SaveChanges();
        //            MailTemplateEntity mailTemplateEntity = _mailTemplateEntity;
        //            mailTemplateEntity.PartitionKey = _mailTemplateEntity.OrgUnitId.ToString();
        //            mailTemplateEntity.RowKey = _mailTemplateEntity.Code.ToString();
        //            _securityTableService.AddEntity(_mailTemplateEntity);
        //            _securityTableService.SaveChanges();
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        string exception = string.Format("{0}\n {1}", e.Message, e.StackTrace);
        //        //Log.Write(EventKind.Error, "Error updating MailTemplate" + exception);
        //    }
        //}

        //public void Delete()
        //{
        //    try
        //    {
        //        _securityTableService.DeleteEntity(_mailTemplateEntity);
        //        _securityTableService.SaveChanges();
        //    }
        //    catch (Exception e)
        //    {
        //        string exception = string.Format("{0}\n {1}", e.Message, e.StackTrace);
        //       // Log.Write(EventKind.Error, "Error deleting MailTemplate" + exception);
        //    }
        //}

        //public bool LoadByKeys(Guid orgUnitId, string code)
        //{
        //    bool retval = false;
        //    //need to add 1==1 since this will throw error if table is empty
        //    var results = from g in _dataContext.MailTemplateEntity
        //                  where 1 == 1 && g.PartitionKey == orgUnitId.ToString() && g.RowKey == code.ToString()
        //                  select g;
        //    try
        //    {
        //        MailTemplateEntity mailTemplateEntity = results.FirstOrDefault<MailTemplateEntity>();

        //        if (mailTemplateEntity != null)
        //        {
        //            Load(mailTemplateEntity);
        //            //Persistent: Huzan
        //            //10-Oct-2011
        //            //Fix 1178
        //            //ResolveLogo();
        //            //Persistent:Deepti. Defect 2068
        //            //ResolveClientLogo(orgUnitId);
        //            retval = true;
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        string exception = string.Format("{0}\n {1}", e.Message, e.StackTrace);
        //        //Log.Write(EventKind.Error, "MailTemplate.LoadByKeys Error Message:" + exception);
        //    }
        //    return retval;
        //}

        //Persistent:Deepti. Defect 2068
        //private void ResolveClientLogo(Guid orgUnitId)
        //{
        //    string blobPath = GetClientImageLogo(orgUnitId);
        //    if (blobPath == string.Empty)
        //    {
        //        string container = String.Format("pub/email-{0}/MediaValet_Logo.png", Guid.Empty.ToString());
        //        blobPath = String.Format("{0}/{1}",
        //            ConfigManager.GetConfigurationSetting(ConfigManager.DefaultBlobStorageEndpointConfigurationString),
        //            container);
        //    }
        //    if (_mailTemplateEntity.BodyContent.Contains("<img src="))
        //    {
        //        Match match = Regex.Match(_mailTemplateEntity.BodyContent, "(<img src=.*?>)");
        //        string oldImageSrc = match.Groups[0].Value;

        //        string newImageSrc = String.Format("<img src=\"{0}\" />", blobPath);

        //        _mailTemplateEntity.BodyContent = _mailTemplateEntity.BodyContent.Replace(oldImageSrc, newImageSrc);
        //    }
        //    else
        //    {
        //        string.Concat(_mailTemplateEntity.BodyContent, "<br />", String.Format("<img src=\"{0}\" />", blobPath));
        //    }
        //}

        //public static string GetClientImageLogo(Guid orgUnitId, int imageType = 3)
        //{
        //    try
        //    {
        //        //If custom templates not implemented for the repo, orgunit used will be Guid.Empty, in this case use the default MV logo 
        //        if (orgUnitId == Guid.Empty)
        //        {
        //            return string.Empty;
        //        }
        //        OrgUnit org = new OrgUnit(orgUnitId.ToString());
        //        string protocol = "https";
        //        string requestedcname = org.DomainName;
        //        string clientLogoBaseUrl = ConfigManager.GetConfigurationSetting("ClientLogoBaseURL");
        //        string clientLogoFileName;
        //        //Adding this check as while sending emails from MP, current is null
        //        if (HttpContext.Current != null)
        //        {
        //            if (HttpContext.Current.User.Identity.IsAuthenticated)
        //            {
        //                clientLogoFileName = OrgUnitPortal.GetOrgUnitLogo(orgUnitId, requestedcname, imageType);
        //            }
        //            else
        //            {
        //                clientLogoFileName = OrgUnitPortal.GetOrgUnitLogo(requestedcname, imageType);
        //            }
        //        }
        //        else
        //        {
        //            clientLogoFileName = OrgUnitPortal.GetOrgUnitLogo(orgUnitId, requestedcname, imageType);
        //           // Log.Write(EventKind.Information, "in else clientLogoFileName : " + clientLogoFileName);
        //        }

        //        string imageLogoUrl = protocol + "://" + clientLogoBaseUrl + "/" + clientLogoFileName;
        //        //Log.Write(EventKind.Information, "in GetClientImageLogo: " + imageLogoUrl);
        //        return imageLogoUrl;
        //    }
        //    catch (Exception ex)
        //    {
        //        //Log.Write(EventKind.Information, "Error in GetClientImageLogo: " + ex.Message + " " + ex.StackTrace);
        //        return string.Empty;
        //    }
        //}

        //public static string GetCurrentDomainName()
        //{

        //    HttpContext currentContext = HttpContext.Current;
        //    string domainName = currentContext.Request.Url.DnsSafeHost;
        //    return domainName;
        //}


        //public static MailTemplate GetTemplateByCode(Guid orgUnitId, string templateCode)
        //{
        //    MailTemplate mailTemplate = new MailTemplate();
        //    if (mailTemplate.LoadByKeys(orgUnitId, templateCode))
        //    {
        //        return mailTemplate;
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

        public static string ResolveTokens(string content, Hashtable keyvalue)
        {
            if (keyvalue == null)
            {
                return content;
            }
            try
            {
                //Input : Dear {USERNAME}, WELCOME TO THE {COMPANYNAME}
                //OutPut : List fo matches which has {anyword}
                //FOr each Item in the List
                //Check whether the item is in the Hashtable.
                //If its in the Hashtable then Replace that Value in content
                //Else Replace the key with empty string   
                object rplcontent = new object();
                string trimmedkeyval = "";
                TemplateParser tp = new TemplateParser();
                tp.FindTagsInTemplateString(content);
                ArrayList KeyValues = new ArrayList(tp.TemplateTags.Keys);
                foreach (string keyval in KeyValues)
                {
                    trimmedkeyval = keyval.Trim(new char[] { '{', '}' });

                    foreach (object key in keyvalue.Keys)
                    {
                        if (keyvalue.ContainsKey(trimmedkeyval))
                        {

                            rplcontent = keyvalue[trimmedkeyval];
                            if (rplcontent != null)
                            {
                                content = content.Replace(keyval, rplcontent.ToString());
                                break;
                            }
                            else
                            {
                                content = content.Replace(keyval, "");
                            }
                        }

                    }
                }


            }
            catch (Exception e)
            {
                string exception = string.Format("{0}\n {1}", e.Message, e.StackTrace);
                //Log.Write(EventKind.Error, " MailTemplateCollection.GetMailTemplateList Error Message:" + exception);
            }
            return content;
        }

        //Persistent: Huzan
        //10-Oct-2011
        //Fix 1178
        //private bool LogoExists()
        //{
        //    bool exists = false;
        //    if (!_mailTemplateEntity.BodyContent.Contains("<img src="))
        //    {
        //        return exists;
        //    }
        //    int startIndex = _mailTemplateEntity.BodyContent.IndexOf("<img src=");
        //    string imageSrc = _mailTemplateEntity.BodyContent.Substring(startIndex);

        //    startIndex = imageSrc.IndexOf("email-");
        //    string fileName = imageSrc.Substring(startIndex);
        //    fileName = fileName.Substring(0, fileName.LastIndexOf("\""));

        //    string blobPath = "pub";

        //    IFileSystem fs = null;
        //    IFileSystemClassFactory fsf = FileSystemProvider.Factory;
        //    fs = fsf.Create(ConfigManager.MediaValetServiceAccountID, blobPath);

        //    return fs.FileExists(string.Format("{0}/{1}", blobPath, fileName));

        //}
        //Persistent: Huzan
        //10-Oct-2011
        //Fix 1178
        //private void ResolveLogo()
        //{
        //    //Persistent: Karuna: Fix for 1178: Always replace the logo with MediaValet logo
        //    //if (!LogoExists())
        //    //{
        //    string container = String.Format("pub/email-{0}/MediaValet_Logo.png", Guid.Empty.ToString());
        //    string blobPath = String.Format("{0}/{1}",
        //        ConfigManager.GetConfigurationSetting(ConfigManager.DefaultBlobStorageEndpointConfigurationString),
        //        container);

        //    if (_mailTemplateEntity.BodyContent.Contains("<img src="))
        //    {
        //        //Persistent: Karuna: Fix for 1178: replace only the image tag
        //        Match match = Regex.Match(_mailTemplateEntity.BodyContent, "(<img src=.*?>)");
        //        string oldImageSrc = match.Groups[0].Value;

        //        string newImageSrc = String.Format("<img src=\"{0}\" />", blobPath);

        //        _mailTemplateEntity.BodyContent = _mailTemplateEntity.BodyContent.Replace(oldImageSrc, newImageSrc);
        //    }
        //    else
        //    {
        //        string.Concat(_mailTemplateEntity.BodyContent, "<br />", String.Format("<img src=\"{0}\" />", blobPath));
        //    }

        //    //}
        //}

        /// <summary>
        /// Resolve the Mediavalet help file 
        /// </summary>
        //public void ResolveHelpFile()
        //{
        //    //persistent:Minal : 1646 :Request User Account from Login page creates new account without Approval
        //    //need to send mail to user for register once admin approves him
        //    string container = String.Format("pub/email-{0}/Mediavalet-Help.pdf", Guid.Empty.ToString()); //Rahul,2370 : Rename helpfile name
        //    string blobPath = String.Format("{0}/{1}",
        //        ConfigManager.GetConfigurationSetting(ConfigManager.DefaultBlobStorageEndpointConfigurationString),
        //        container);

        //    if (_mailTemplateEntity.BodyContent.Contains("<a id=\"helpfile\" href="))
        //    {

        //        Match match = Regex.Match(_mailTemplateEntity.BodyContent, "(<a id=\"helpfile\" href=.*?>)");
        //        string oldHrefSrc = match.Groups[0].Value;

        //        //Persistent: Deepti. defect 1802. removed "/>" from <a> TAG as </a> is already there in the mail templete
        //        string newHrefSrc = String.Format("<a id=\"helpfile\" href=\"{0}\" target=\"_blank\">", blobPath);

        //        _mailTemplateEntity.BodyContent = _mailTemplateEntity.BodyContent.Replace(oldHrefSrc, newHrefSrc);
        //    }
        //    else
        //    {
        //        //Persistent: Deepti. defect 1802. removed "/>" from <a> TAG as </a> is already there in the mail templete
        //        string.Concat(_mailTemplateEntity.BodyContent, "<br />", String.Format("<a id=\"helpfile\" href=\"{0}\" target=\"_blank\">", blobPath));
        //    }


        //}

        //Persistent: Karuna: Change the logo for the respective organization
        //public void PutClientLogo(Guid orgUnidId)
        //{
        //    if (ClientLogoexists(orgUnidId))
        //    {
        //        //Replace the img tag with client logo
        //        string container = String.Format("pub/email-{0}/logo.png", orgUnidId.ToString());
        //        string blobPath = String.Format("{0}/{1}",
        //            ConfigManager.GetConfigurationSetting(ConfigManager.DefaultBlobStorageEndpointConfigurationString),
        //            container);

        //        if (_mailTemplateEntity.BodyContent.Contains("<img src="))
        //        {
        //            //Persistent: Karuna: Fix for 1178: replace only the image tag
        //            Match match = Regex.Match(_mailTemplateEntity.BodyContent, "(<img src=.*?>)");
        //            string oldImageSrc = match.Groups[0].Value;

        //            string newImageSrc = String.Format("<img src=\"{0}\" />", blobPath);

        //            _mailTemplateEntity.BodyContent = _mailTemplateEntity.BodyContent.Replace(oldImageSrc, newImageSrc);
        //        }
        //        else
        //        {
        //            string.Concat(_mailTemplateEntity.BodyContent, "<br />", String.Format("<img src=\"{0}\" />", blobPath));
        //        }
        //    }
        //    else
        //    {
        //        //Put the default mediavalet logo
        //        string container = String.Format("pub/email-{0}/logo.png", Guid.Empty.ToString());
        //        string blobPath = String.Format("{0}/{1}",
        //            ConfigManager.GetConfigurationSetting(ConfigManager.DefaultBlobStorageEndpointConfigurationString),
        //            container);

        //        if (_mailTemplateEntity.BodyContent.Contains("<img src="))
        //        {
        //            //Persistent: Karuna: Fix for 1178: replace only the image tag
        //            Match match = Regex.Match(_mailTemplateEntity.BodyContent, "(<img src=.*?>)");
        //            string oldImageSrc = match.Groups[0].Value;

        //            string newImageSrc = String.Format("<img src=\"{0}\" />", blobPath);

        //            _mailTemplateEntity.BodyContent = _mailTemplateEntity.BodyContent.Replace(oldImageSrc, newImageSrc);
        //        }
        //        else
        //        {
        //            string.Concat(_mailTemplateEntity.BodyContent, "<br />", String.Format("<img src=\"{0}\" />", blobPath));
        //        }
        //    }
        //}

        //Persistent: Karuna: Fix for 1178: 
        //private bool ClientLogoexists(Guid orgUnidId)
        //{
        //    string fileName = "email-" + orgUnidId.ToString() + "/logo.png";

        //    string blobPath = "pub";

        //    IFileSystem fs = null;
        //    IFileSystemClassFactory fsf = FileSystemProvider.Factory;
        //    fs = fsf.Create(ConfigManager.MediaValetServiceAccountID, blobPath);

        //    return fs.FileExists(string.Format("{0}/{1}", blobPath, fileName));
        //}

        public class TemplateTag
        {
            public TemplateTag()
            {
                _tag = string.Empty;
                _value = string.Empty;
            }
            public TemplateTag(string Tag, string Value)
            {
                _tag = Tag;
                _value = Value;
            }
            public event EventHandler TagChanged;
            protected virtual void OnTagChanged()
            {
                if (TagChanged != null)
                    TagChanged(this, EventArgs.Empty);
            }

            private string _tag;
            public string Tag
            {
                get { return _tag; }
                set
                {
                    if (_tag != value)
                    {
                        _tag = value;
                        OnTagChanged();
                    }
                }
            }

            public event EventHandler ValueChanged;
            protected virtual void OnValueChanged()
            {
                if (ValueChanged != null)
                    ValueChanged(this, EventArgs.Empty);
            }

            private string _value;
            public string Value
            {
                get { return _value; }
                set
                {
                    if (_value != value)
                    {
                        _value = value;
                        OnValueChanged();
                    }
                }
            }
        }

        public class TemplateParser
        {
            public TemplateParser() { }
            public void AddTag(TemplateTag templateTag)
            {
                _templateTags[templateTag.Tag] = templateTag;
            }
            public void AddTag(string Tag)
            {
                AddTag(new TemplateTag(Tag, string.Empty));
            }
            public void AddTag(string Tag, string Value)
            {
                AddTag(new TemplateTag(Tag, Value));
            }


            public void RemoveTag(string Tag)
            {
                _templateTags.Remove(Tag);
            }


            public void ClearTags()
            {
                _templateTags.Clear();
            }


            private string _replaceTagHandler(Match token)
            {
                if (_templateTags.Contains(token.Value))
                    return ((TemplateTag)_templateTags[token.Value]).Value;
                else
                    return string.Empty;
            }

            public string ParseTemplateString(string Template)
            {
                MatchEvaluator replaceCallback = new MatchEvaluator(_replaceTagHandler);
                string newString = Regex.Replace(Template, _matchPattern, replaceCallback);
                return newString;
            }

            public void FindTagsInTemplateString(string Template)
            {
                MatchCollection tags = Regex.Matches(Template, _matchPattern);

                foreach (Match tag in tags) AddTag(tag.ToString());
            }

            private string _matchPattern = @"\{\w+\}";
            public string MatchPattern
            {
                get { return _matchPattern; }
                set { _matchPattern = value; }
            }
            private Hashtable _templateTags = new Hashtable();
            public Hashtable TemplateTags
            {
                get { return _templateTags; }
            }
        }



    }
}
