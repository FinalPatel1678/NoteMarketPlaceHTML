using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using NotesMarketplace.Models;
using PagedList;

namespace NotesMarketplace.Controllers
{
    public class HomeController : Controller
    {
        static int userid = 0;
        static String route = null;
        private NotesMarketplaceEntities db = new NotesMarketplaceEntities();

        public ActionResult Home()
        {
            ViewBag.HomePage = "true";
            if (userid != 0)
            {
                ViewBag.IsValid = "true";
                var userprofile = db.UserProfiles.FirstOrDefault(m => m.F_K_User == userid);
                if (userprofile != null)
                {
                    ViewBag.ProfilePicture = userprofile.ProfilePicture;
                }
            }
            return View();
        }

        public ActionResult AddNotes(String note_edit)
        {
            if (userid != 0)
            {
                ViewBag.IsValid = "true";
                var userprofile = db.UserProfiles.FirstOrDefault(m => m.F_K_User == userid);
                if (userprofile != null)
                {
                    ViewBag.ProfilePicture = userprofile.ProfilePicture;
                }

                List<ManageCTC> manageCTCs = db.ManageCTCs.ToList();
                ViewBag.MyCTC = manageCTCs;

                if (note_edit != null)
                {
                    if (note_edit.Length != 0)
                    {
                        NotesDetail notesDetail = db.NotesDetails.FirstOrDefault(m => m.P_K_Note.ToString().Equals(note_edit));
                        if (notesDetail != null)
                        {
                            ViewBag.editmode = "true";
                            return View(notesDetail);
                        }
                    }
                }
                return View(new NotesDetail());
            }
            else
            {
                return RedirectToAction("Login", "Home");
            }
        }

        
        public ActionResult PublishNote(NotesDetail notesDetail)
        {
            if (notesDetail != null)
            {
                notesDetail.F_K_NoteStatus = 3;
                return RedirectToAction("AddNotes", "Home", new { notesDetail = notesDetail});
               // return AddNotes(notesDetail);
            }
            return AddNotes("");
        }

        public ActionResult SaveNote(NotesDetail notesDetail)
        {
            if (notesDetail != null)
            {
                notesDetail.F_K_NoteStatus = 1;
                return AddNotes(notesDetail);
            }
            else
            {
                return AddNotes("");
            }

        }

        [HttpPost]
        public ActionResult AddNotes(NotesDetail notesDetail)
        {
            if (userid != 0)
            {
                List<ManageCTC> manageCTCs = db.ManageCTCs.ToList();
                ViewBag.MyCTC = manageCTCs;
                if (notesDetail != null)
                {
                    if (notesDetail.Category.Equals("Select Your category"))
                    {
                        ViewBag.category = "Select category of note";
                        notesDetail.SellPrice = 0;
                        return View(notesDetail);
                    }
                    if (notesDetail.NoteType.Equals("Select your mode type"))
                    {
                        notesDetail.NoteType = null;
                    }
                    if (notesDetail.Country.Equals("Select your country"))
                    {
                        notesDetail.Country = null;
                    }
                    if (notesDetail.Note_Attachment == null && notesDetail.NoteAttachment == null)
                    {
                        ViewBag.Note_Attachmen = "Please attach your notes here...";
                        notesDetail.SellPrice = 0;
                        return View(notesDetail);
                    }
                    notesDetail.F_K_User = userid;
                    notesDetail.IsActive = true;
                    notesDetail.CreatedDate = DateTime.Now;
                    if (notesDetail.Book_Picture != null && (notesDetail.BookPicture == null))
                    {
                        if (notesDetail.Book_Picture.ContentLength > 1024 * 1024 * 10)
                        {
                            ViewBag.Book_Picture = "File size should be less then 10 MB!";
                            notesDetail.SellPrice = 0;
                            return View(notesDetail);
                        }
                        string path = Server.MapPath("~/Uploads/BookPicture/");
                        String extension = Path.GetExtension(notesDetail.Book_Picture.FileName);
                        var supportedTypes = new[] { ".jpg", ".jpeg", ".png" };
                        if (supportedTypes.Contains(extension.ToUpper()) || supportedTypes.Contains(extension.ToLower()))
                        {
                            string fileName = userid.ToString() + DateTime.Now.ToString("G").Replace(" ", String.Empty).Replace("-", String.Empty).Replace(":", String.Empty) + extension;
                            notesDetail.Book_Picture.SaveAs(Path.Combine(path, fileName));
                            notesDetail.BookPicture = fileName;
                        }
                        else
                        {
                            ViewBag.Book_Picture = "Choose images only";
                            notesDetail.SellPrice = 0;
                            return View(notesDetail);
                        }
                    }
                    if (notesDetail.Note_Attachment != null && notesDetail.NoteAttachment == null)
                    {
                        string path = Server.MapPath("~/Uploads/Books/");
                        String extension = Path.GetExtension(notesDetail.Note_Attachment.FileName);
                        var supportedTypes = new[] { ".pdf" };
                        if (supportedTypes.Contains(extension.ToUpper()) || supportedTypes.Contains(extension.ToLower()))
                        {
                            string fileName = userid.ToString() + DateTime.Now.ToString("G").Replace(" ", String.Empty).Replace("-", String.Empty).Replace(":", String.Empty) + extension;
                            notesDetail.Note_Attachment.SaveAs(Path.Combine(path, fileName));
                            notesDetail.NoteAttachment = fileName;
                            notesDetail.NoteSize = notesDetail.Note_Attachment.ContentLength;
                        }
                        else
                        {
                            ViewBag.Note_Attachment = "Choose pdf file only";
                            notesDetail.SellPrice = 0;
                            return View(notesDetail);
                        }
                    }
                    if (notesDetail.SellPrice != 0 && notesDetail.Note_Preview == null)
                    {
                        ViewBag.Note_Preview = "Pleasse add note preview here...";
                        notesDetail.SellPrice = 0;
                        return View(notesDetail);
                    }
                    if (notesDetail.Note_Preview != null && notesDetail.Note_Preview == null)
                    {
                        if (notesDetail.Note_Preview.ContentLength > 1024 * 1024 * 10)
                        {
                            ViewBag.Note_Preview = "File size should be less then 10 MB!";
                            notesDetail.SellPrice = 0;
                            return View(notesDetail);
                        }
                        string path = Server.MapPath("~/Uploads/BookPreview/");
                        String extension = Path.GetExtension(notesDetail.Note_Preview.FileName);
                        var supportedTypes = new[] { ".pdf" };
                        if (supportedTypes.Contains(extension.ToUpper()) || supportedTypes.Contains(extension.ToLower()))
                        {
                            string fileName = userid.ToString() + DateTime.Now.ToString("G").Replace(" ", String.Empty).Replace("-", String.Empty).Replace(":", String.Empty) + extension;
                            notesDetail.Note_Preview.SaveAs(Path.Combine(path, fileName));
                            notesDetail.NotePreview = fileName;
                        }
                        else
                        {
                            ViewBag.Note_Preview = "Choose pdf only";
                            notesDetail.SellPrice = 0;
                            return View(notesDetail);
                        }
                    }
                    try
                    {
                        if (notesDetail.F_K_NoteStatus == 3 && notesDetail.P_K_Note == 0)
                        {
                            User user = db.Users.FirstOrDefault(m => m.P_K_User == userid);
                            MailMessage mail = new MailMessage("final16700@gmail.com", user.EmailId.ToString());
                            mail.Subject = user.FirstName + " " + user.LastName + " sent his note for review";
                            string Body = "Hello Admins, \n\nWe want to inform you that, " + user.FirstName + " " + user.LastName + " sent his note " + notesDetail.Title + " for review.Please look at the notes and take required actions.\n\nRegards,\nNotes Marketplace";
                            mail.Body = Body;
                            mail.IsBodyHtml = true;

                            SmtpClient smtp = new SmtpClient();
                            smtp.Host = "smtp.gmail.com";
                            smtp.Port = 587;
                            smtp.UseDefaultCredentials = false;

                            NetworkCredential nc = new NetworkCredential("final16700@gmail.com", "12345678finalpatel");
                            smtp.EnableSsl = true;
                            smtp.Credentials = nc;
                            smtp.Send(mail);
                        }
                    }
                    catch
                    {

                    }

                    if (notesDetail.P_K_Note == 0)
                    {
                        db.NotesDetails.Add(notesDetail);
                        db.SaveChanges();
                    }
                    else
                    {
                        NotesDetail notesDetail1 = db.NotesDetails.FirstOrDefault(m => m.P_K_Note == notesDetail.P_K_Note);
                        if (notesDetail1 != null)
                        {
                            notesDetail1.F_K_NoteStatus = notesDetail.F_K_NoteStatus;
                            notesDetail1.F_K_User = notesDetail.F_K_User;
                            notesDetail1.BookPicture = notesDetail.BookPicture;
                            notesDetail1.Category = notesDetail.Category;
                            notesDetail1.Country = notesDetail.Country;
                            notesDetail1.Course = notesDetail.Course;
                            notesDetail1.CourseCode = notesDetail.CourseCode;
                            notesDetail1.InstitutionName = notesDetail.InstitutionName;
                            notesDetail1.ModifiedDate = DateTime.Now;
                            notesDetail1.NoteAttachment = notesDetail.NoteAttachment;
                            notesDetail1.NotePreview = notesDetail.NotePreview;
                            notesDetail1.NotesDescription = notesDetail1.NotesDescription;
                            notesDetail1.NoteSize = notesDetail.Note_Attachment.ContentLength;
                            notesDetail1.NoteType = notesDetail.NoteType;
                            notesDetail1.NumberOfPages = notesDetail.NumberOfPages;
                            notesDetail1.Professor = notesDetail.Professor;
                            notesDetail1.SellPrice = notesDetail.SellPrice;
                            notesDetail1.Title = notesDetail.Title;
                            db.SaveChanges();
                        }
                    }

                }
                else
                {
                    return View();
                }
                return RedirectToAction("Dashboard", "Home");
            }
            else
            {
                return HttpNotFound();
            }
        }

        [ChildActionOnly]
        public ActionResult Submitted(String field_2, String search_progress_2, int? pageindex_2)
        {
            List<NotesDetail> mynotes = db.NotesDetails.Where(m => m.F_K_User == userid && (m.F_K_NoteStatus == 4) && m.IsActive).OrderByDescending(m => m.CreatedDate).ToList();

            if (!String.IsNullOrEmpty(field_2))
            {
                if (field_2.Equals("title"))
                {
                    mynotes = mynotes.OrderBy(m => m.Title).ToList();
                }
                if (field_2.Equals("category"))
                {
                    mynotes = mynotes.OrderBy(m => m.Category).ToList();
                }
            }

            if (search_progress_2 != null && search_progress_2.Length != 0)
            {
                mynotes = mynotes.Where(m => m.F_K_User == userid && (m.Title.ToLower().StartsWith(search_progress_2.ToLower()) || m.Category.ToLower().StartsWith(search_progress_2.ToLower()))).ToList();
            }
            List<NotesStatu> notesStatus = db.NotesStatus.ToList();
            ViewBag.status = notesStatus;
            return PartialView(mynotes.ToPagedList(pageindex_2 ?? 1, 5));
        }

        [ChildActionOnly]
        public ActionResult InProgress(String field, String search_progress, String note_delete, int? pageindex)
        {
            List<NotesDetail> mynotes = db.NotesDetails.Where(m => m.F_K_User == userid && (m.F_K_NoteStatus == 1 || m.F_K_NoteStatus == 2 || m.F_K_NoteStatus == 3) && m.IsActive).OrderByDescending(m => m.CreatedDate).ToList();

            if (note_delete != null)
            {
                NotesDetail notesDetail = db.NotesDetails.FirstOrDefault(m => m.P_K_Note.ToString().Equals(note_delete));

                if (notesDetail != null)
                {
                    notesDetail.IsActive = false;
                }
                db.SaveChanges();
            }
            if (!String.IsNullOrEmpty(field))
            {
                if (field.Equals("title"))
                {
                    mynotes = mynotes.OrderBy(m => m.Title).ToList();
                }
                if (field.Equals("category"))
                {
                    mynotes = mynotes.OrderBy(m => m.Category).ToList();
                }
                if (field.Equals("status"))
                {
                    mynotes = mynotes.OrderBy(m => m.NotesStatu.Name).ToList();
                }
            }

            if (search_progress != null && search_progress.Length != 0)
            {
                mynotes = mynotes.Where(m => m.F_K_User == userid && (m.Title.ToLower().StartsWith(search_progress.ToLower()) || m.Category.ToLower().StartsWith(search_progress.ToLower()) || m.NotesStatu.Name.ToLower().StartsWith(search_progress.ToLower()))).ToList();
            }

            List<NotesStatu> notesStatus = db.NotesStatus.ToList();
            ViewBag.status = notesStatus;
            return PartialView(mynotes.ToPagedList(pageindex ?? 1, 5));
        }

        public ActionResult Dashboard()
        {
            route = "Dashboard";
            if (userid != 0)
            {
                ViewBag.IsValid = "true";
                var userprofile = db.UserProfiles.FirstOrDefault(m => m.F_K_User == userid);
                if (userprofile != null)
                {
                    ViewBag.ProfilePicture = userprofile.ProfilePicture;
                }

                Statistic statistic = db.Statistics.FirstOrDefault(m => m.F_K_User == userid);
                ViewBag.statistic = statistic;
                return View();
            }
            else
            {
                return RedirectToAction("Login", "Home");
            }
        }

        public ActionResult BuyerRequests(int? pageindex,String search, String shorthead)
        {
            route = "BuyerRequests";
            if (userid != 0)
            {
                ViewBag.IsValid = "true";
                var userprofile = db.UserProfiles.FirstOrDefault(m => m.F_K_User == userid);
                if (userprofile != null)
                {
                    ViewBag.ProfilePicture = userprofile.ProfilePicture;
                }
                
                List<DownloadRequests> requests = new List<DownloadRequests>();
                List<DownloadedNote> downloadedNotes = db.DownloadedNotes.Where(m => m.IsApproved == false && m.F_K_User_Seller==userid).ToList();
                foreach(DownloadedNote myrequest in downloadedNotes)
                {
                    DownloadRequests dr = new DownloadRequests();
                    dr.downloadedNote = myrequest;
                    dr.phonenumber= db.UserProfiles.FirstOrDefault(m => m.F_K_User == myrequest.F_K_User_Buyer).PhoneNumber;
                    dr.emailid = db.Users.FirstOrDefault(m => m.P_K_User == myrequest.F_K_User_Buyer).EmailId;

                    requests.Add(dr);
                }

                requests = requests.OrderByDescending(m => m.downloadedNote.CreatedDate).ToList();

                if(!String.IsNullOrEmpty(search))
                {
                    requests = requests.Where(m =>m.downloadedNote.Title.ToLower().StartsWith(search.ToLower()) || m.downloadedNote.SellPrice.ToString().Equals(search.ToString()) || m.downloadedNote.Category.ToLower().StartsWith(search)|| m.emailid.Equals(search)||m.phonenumber.ToString().Equals(search.ToString())).ToList();
                }
                if(!String.IsNullOrEmpty(shorthead))
                {
                    if (shorthead.Equals("title"))
                    {
                        requests = requests.OrderBy(m => m.downloadedNote.Title).ToList();
                    }
                    if (shorthead.Equals("category"))
                    {
                        requests = requests.OrderBy(m => m.downloadedNote.Category).ToList();

                    }
                    if(shorthead.Equals("phonenumber"))
                    {

                        requests = requests.OrderBy(m => m.phonenumber).ToList();
                    }
                    if(shorthead.Equals("buyer"))
                    {

                        requests = requests.OrderBy(m => m.emailid).ToList();
                    }
                    if(shorthead.Equals("price"))
                    {
                        requests = requests.OrderBy(m => m.downloadedNote.SellPrice).ToList();
                    }
                }
                return View(requests.ToPagedList(pageindex ?? 1,10));
            }
            else
            {
                return RedirectToAction("Login", "Home");
            }
        }

        public ActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SignUp(User user)
        {
            var isEmailAlreadyExists = db.Users.Any(x => x.EmailId == user.EmailId);
            if (isEmailAlreadyExists)
            {
                ViewBag.flag = "Exits";
                return View(user);
            }
            else
            {
                user.F_K_UserRoles = 1;
                user.IsActive = true;
                user.CreaatedDate = DateTime.Now;
                Statistic statistic = new Statistic();
                statistic.F_K_User = user.P_K_User;
                db.Statistics.Add(statistic);
                db.Users.Add(user);
                try
                {
                    db.SaveChanges();
                    MailMessage mail = new MailMessage("final16700@gmail.com", user.EmailId.ToString());
                    mail.Subject = "Note Marketplace - Email Verification";
                    string Body = "Hello " + user.FirstName + " " + user.LastName + ",\n\nThank you for signing up with us. Please click on below link to verify your email address and to do login. \nhttps://localhost:44381/Home/EmailVerification/?id=" + user.EmailId + "\n\nRegards,\nNotes Marketplace";
                    mail.Body = Body;
                    mail.IsBodyHtml = true;

                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = "smtp.gmail.com";
                    smtp.Port = 587;
                    smtp.UseDefaultCredentials = false;

                    NetworkCredential nc = new NetworkCredential("final16700@gmail.com", "12345678finalpatel");
                    smtp.EnableSsl = true;
                    smtp.Credentials = nc;
                    smtp.Send(mail);
                    return RedirectToAction("Login", "Home");
                }
                catch (Exception e)
                {
                    return View(user);
                }
            }
        }

        public ActionResult Login()
        {
            HttpCookie cookieObj = Request.Cookies["RememberMe"];
            User user = new User();

            if (cookieObj != null)
            {
                user.EmailId = cookieObj["emailid"];
                user.Password = cookieObj["password"];

                if (cookieObj["emailid"].Length != 0)
                {
                    user.RememberMe = true;
                }
                else
                {
                    user.RememberMe = false;
                }
            }

            return View(user);
        }

        [HttpPost]
        public ActionResult Login(User user)
        {
            HttpCookie cookie = new HttpCookie("RememberMe");
            if (user.RememberMe == true)
            {
                cookie["emailid"] = user.EmailId;
                cookie["password"] = user.Password;
                cookie.Expires = DateTime.Now.AddMonths(1);
                Response.Cookies.Add(cookie);
            }
            else
            {
                cookie["emailid"] = null;
                cookie["password"] = null;
                Response.Cookies.Add(cookie);
            }

            var auth = db.Users.Where(a => a.EmailId.Equals(user.EmailId) && a.Password.Equals(user.Password)).FirstOrDefault();
            if (auth != null)
            {
                if (auth.IsActive == true)
                {
                    if (auth.F_K_UserRoles == 1)
                    {
                        if (auth.IsEmailVerified == true)
                        {
                            userid = auth.P_K_User;
                            if (auth.IsDetailsSubmitted == true)
                            {
                                if(!String.IsNullOrEmpty(route))
                                {
                                    return RedirectToAction(route, "Home");
                                }
                                return RedirectToAction("Home", "Home");
                            }
                            else
                            {
                                return RedirectToAction("UserProfile", "Home");
                            }
                        }
                        else
                        {
                            ViewBag.isauth = "Varify your Email account for login";
                            return View(user);
                        }
                    }
                    else
                    {
                        ViewBag.isauth = "You are not Member...";
                    }
                }
                ViewBag.isauth = "You are Blocked...";
            }
            else
            {
                ViewBag.isauth = "The password that you've entered is incorrect";
            }
            return View(user);
        }

        public ActionResult FAQ()
        {
            route = "FAQ";
            if (userid != 0)
            {
                ViewBag.IsValid = "true";
                var userprofile = db.UserProfiles.FirstOrDefault(m => m.F_K_User == userid);
                if (userprofile != null)
                {
                    ViewBag.ProfilePicture = userprofile.ProfilePicture;
                }
            }
            List<FAQ> myfaqs = db.FAQs.ToList();
            return View(myfaqs);
        }

        public FileResult DownloadFile(string fileName)
        {
            string path = Server.MapPath("~/Uploads/Books/") + fileName;
            byte[] bytes = System.IO.File.ReadAllBytes(path);
            return File(bytes, "application/octet-stream", fileName);
        }

        public ActionResult NoteDetails(String noteid, String SellPrice)
        {
            if (userid != 0)
            {
                ViewBag.IsValid = "true";
                var userprofile = db.UserProfiles.FirstOrDefault(m => m.F_K_User == userid);
                if (userprofile != null)
                {
                    ViewBag.ProfilePicture = userprofile.ProfilePicture;
                }
            }

            if (!String.IsNullOrEmpty(noteid))
            {
                NotesDetail noteDetails = db.NotesDetails.FirstOrDefault(m => m.P_K_Note.ToString().Equals(noteid));
                if (noteDetails != null)
                { 
                    if(SellPrice!=null)
                    {
                        if (userid == noteDetails.F_K_User)
                        {
                            return RedirectToAction("DownloadFile", "Home", new { filename = noteDetails.NoteAttachment });
                        }
                        if (userid==0)
                        {
                            return RedirectToAction("Login","Home");
                        }
                        DownloadedNote downloadedNote = new DownloadedNote();
                        downloadedNote.F_K_Note = noteDetails.P_K_Note;
                        downloadedNote.F_K_User_Buyer = userid;
                        downloadedNote.F_K_User_Seller = noteDetails.F_K_User;
                        downloadedNote.IsActive = true;
                        downloadedNote.Title = noteDetails.Title;
                        downloadedNote.Attachment = noteDetails.NoteAttachment;
                        downloadedNote.Category = noteDetails.Category;
                        
                        bool isexist = db.DownloadedNotes.Any(m => m.F_K_Note == noteDetails.P_K_Note && m.F_K_User_Buyer == userid && m.F_K_User_Seller == noteDetails.F_K_User);
                       
                        if (SellPrice.Equals("0"))
                        {
                            if (!isexist)
                            {
                                downloadedNote.SellPrice = 0;
                                downloadedNote.CreatedDate = DateTime.Now;
                                downloadedNote.IsApproved = true;
                                db.DownloadedNotes.Add(downloadedNote);
                                db.SaveChanges();
                            }
                            return RedirectToAction("DownloadFile", "Home", new { filename = noteDetails.NoteAttachment });
                        }
                        else
                        {
                            User user = db.Users.FirstOrDefault(m => m.P_K_User == noteDetails.F_K_User);
                            ViewBag.sellername = user.FirstName+" "+user.LastName;
                            User user2 = db.Users.FirstOrDefault(m => m.P_K_User == userid);
                            ViewBag.name = user2.FirstName;

                            if (isexist)
                            {
                                bool isapproved = db.DownloadedNotes.Any(m => m.F_K_Note == noteDetails.P_K_Note && m.F_K_User_Buyer == userid && m.F_K_User_Seller == noteDetails.F_K_User && m.IsApproved == true);
                                if (isapproved)
                                {
                                    return RedirectToAction("DownloadFile", "Home", new { filename = noteDetails.NoteAttachment });
                                }
                                else
                                {

                                    ViewBag.NotApproved = "true";
                                }
                            }
                            else
                            {
                                ViewBag.NotApproved = "true";
                                downloadedNote.SellPrice = noteDetails.SellPrice;
                                downloadedNote.CreatedDate = DateTime.Now;
                                db.DownloadedNotes.Add(downloadedNote);
                                db.SaveChanges();
                            }
                        }
                    }

                     ViewBag.numofreview = db.Feedbacks.Where(m => m.F_K_Note.ToString().Equals(noteid)).Count();
                     ViewBag.inappropriate = db.SpamReports.Where(m => m.F_K_Note.ToString().Equals(noteid)).Count();
                    return View(noteDetails);
                }
            }
            return HttpNotFound();
        }

        public ActionResult Search(int? pageindex, String search, String university, String type, String country, String course, String rating, String category)
        {
            route = "Search";
            List<NotesDetail> notes = db.NotesDetails.Where(v => v.F_K_NoteStatus == 4 && v.IsActive).ToList();
            if (userid != 0)
            {
                ViewBag.IsValid = "true";
                var userprofile = db.UserProfiles.FirstOrDefault(m => m.F_K_User == userid);
                if (userprofile != null)
                {
                    ViewBag.ProfilePicture = userprofile.ProfilePicture;
                }
            }
            if (!String.IsNullOrEmpty(search))
            {
                notes = notes.Where(m => m.Title.ToLower().StartsWith(search.ToLower())).ToList();
            }
            if (!String.IsNullOrEmpty(university))
            {
                notes = notes.Where(m => m.InstitutionName.ToLower().Equals(university.ToLower())).ToList();
            }
            if (!String.IsNullOrEmpty(type))
            {
                notes = notes.Where(m => m.NoteType.ToLower().Equals(type.ToLower())).ToList();
            }
            if (!String.IsNullOrEmpty(country))
            {
                notes = notes.Where(m => m.Country.ToLower().Equals(country.ToLower())).ToList();
            }
            if (!String.IsNullOrEmpty(course))
            {
                notes = notes.Where(m => m.Course.ToLower().Equals(course.ToLower())).ToList();
            }
            if (!String.IsNullOrEmpty(category))
            {
                notes = notes.Where(m => m.Category.ToLower().Equals(category.ToLower())).ToList();
            }
            if (!String.IsNullOrEmpty(rating))
            {
                // notes = notes.Where(a=>a.Feedbacks.Where(m=>m.F_K_Note==a.P_K_Note).Average(k=>k.Review).ToString().Equals(rating)).ToList();
            }
            ViewBag.count = notes.Count();
            return View(notes.ToPagedList(pageindex ?? 1, 9));
        }

        public ActionResult UserProfile()
        {
            UserProfile userprofile = new UserProfile();
            if (userid != 0)
            {
                User user = db.Users.FirstOrDefault(m => m.P_K_User == userid);
                userprofile.User = user;
                if (userprofile != null)
                {
                    List<ManageCTC> manageCTCs = db.ManageCTCs.ToList();
                    ViewBag.MyCTC = manageCTCs;
                    return View(userprofile);
                }
            }
            return HttpNotFound();
        }

        [HttpPost]
        public ActionResult UserProfile(UserProfile userprofile)
        {
            try
            {
                List<ManageCTC> manageCTCs = db.ManageCTCs.ToList();
                ViewBag.MyCTC = manageCTCs;
                userprofile.PhoneNumber = userprofile.CountryCode.ToString() + userprofile.Number.ToString();
                userprofile.F_K_User = userid;
                if (userprofile.Gender.Equals("Select your gender"))
                {
                    userprofile.Gender = null;
                }
                User user = db.Users.FirstOrDefault(m => m.P_K_User == userid);
                user.IsDetailsSubmitted = true;
                user.Password2 = user.Password;
                user.FirstName = userprofile.User.FirstName;
                user.LastName = userprofile.User.LastName;
                user.IsActive = true;
                userprofile.ModifiedDate = DateTime.Now;
                userprofile.IsActive = true;
                userprofile.ID = user.P_K_User;
                userprofile.User = user;

                string path = Server.MapPath("~/Uploads/ProfilePicture/");
                if (userprofile.File != null)
                {
                    String extension = Path.GetExtension(userprofile.File.FileName);
                    var supportedTypes = new[] { ".jpg", ".jpeg", ".png" };
                    if (supportedTypes.Contains(extension.ToUpper()) || supportedTypes.Contains(extension.ToLower()))
                    {
                        if (userprofile.File.ContentLength > 1024 * 1024 * 10)
                        {
                            ViewBag.ProfilePicture = "File size should be less then 10 MB";
                            //User user2 = db.Users.FirstOrDefault(m => m.P_K_User == userid);
                            return View(userprofile);
                        }
                        string fileName = userid.ToString() + "hfhbshvbadyig" + extension;
                        userprofile.File.SaveAs(Path.Combine(path, fileName));
                        userprofile.ProfilePicture = fileName;
                    }
                    else
                    {
                        ViewBag.ProfilePicture = "Choose image only";
                        // User user2 = db.Users.FirstOrDefault(m => m.P_K_User == userid);
                        return View(userprofile);
                    }
                }

                db.UserProfiles.Add(userprofile);
                db.SaveChanges();

                return RedirectToAction("Search", "Home");
            }
            catch (Exception)
            {
                return View(userprofile);
            }
        }

        public ActionResult ContactUs()
        {
            route = "ContactUs";
            if (userid != 0)
            {
                ViewBag.IsValid = "true";
                var userprofile = db.UserProfiles.FirstOrDefault(m => m.F_K_User == userid);
                if (userprofile != null)
                {
                    ViewBag.ProfilePicture = userprofile.ProfilePicture;
                }
                ContactUs contactus = new ContactUs();
                contactus.FullName = userprofile.User.FirstName + " " + userprofile.User.LastName;
                contactus.EmailId = userprofile.User.EmailId;
                if (contactus.EmailId != null)
                {
                    ViewBag.Email_Disable = "true";
                }
                return View(contactus);
            }
            return View();
        }

        [HttpPost]
        public ActionResult ContactUs(ContactUs contactUs)
        {
            try
            {
                MailMessage mail = new MailMessage("final16700@gmail.com", "final16700@gmail.com");
                mail.Subject = contactUs.Subject.ToString();
                string Body = "Hello,\n" + contactUs.Comments.ToString() + "Regards,\n" + contactUs.FullName;
                mail.Body = Body;
                mail.IsBodyHtml = true;

                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.UseDefaultCredentials = false;

                NetworkCredential nc = new NetworkCredential("final16700@gmail.com", "12345678finalpatel");
                smtp.EnableSsl = true;
                smtp.Credentials = nc;
                smtp.Send(mail);
            }
            catch (Exception e)
            {

            }
            return View();
        }

        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ForgotPassword(string EmailId)
        {
            var auth = db.Users.Where(a => a.EmailId.Equals(EmailId)).FirstOrDefault();

            if (auth != null)
            {
                String getranpass = RandomPassword.Generate(8, 10);
                MailMessage mail = new MailMessage("final16700@gmail.com", EmailId.ToString());
                mail.Subject = "New Temporary Password has been created for you";
                string Body = "Hello,\n\nWe have generated a new password for you \nPassword:" + getranpass + "\n\nRegards,\nNotes Marketplace";
                mail.Body = Body;
                mail.IsBodyHtml = true;

                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.UseDefaultCredentials = false;

                NetworkCredential nc = new NetworkCredential("final16700@gmail.com", "12345678finalpatel");
                smtp.EnableSsl = true;
                smtp.Credentials = nc;
                smtp.Send(mail);

                User user = db.Users.FirstOrDefault(x => x.EmailId.Equals(EmailId.ToString()));
                user.Password = getranpass;
                user.Password2 = getranpass;
                db.SaveChanges();

                return RedirectToAction("Login", "Home");
            }
            return RedirectToAction("Login", "Home");
        }

        public ActionResult EmailVerification(String id)
        {
            try
            {
                User user = db.Users.FirstOrDefault(x => x.EmailId.Equals(id.ToString()));
                if (user != null)
                {
                    return View(user);
                }
                return HttpNotFound();
            }
            catch (Exception e)
            {
                return HttpNotFound();
            }

        }

        [HttpPost]
        public ActionResult EmailVerification(int P_K_User)
        {
            try
            {
                User user = db.Users.FirstOrDefault(x => x.P_K_User == P_K_User);
                if (user != null)
                {
                    user.IsEmailVerified = true;
                    user.Password2 = user.Password;
                    db.SaveChanges();
                    return View("Login");
                }
                return HttpNotFound();
            }
            catch (Exception e)
            {
                return HttpNotFound();
            }

        }

        public ActionResult LogOut()
        {
            route = null;
            userid = 0;
            return RedirectToAction("Home", "Home");
        }
    }
}