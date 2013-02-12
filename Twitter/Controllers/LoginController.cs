using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Twitter.Application;
using Twitter.Models.Account;
using Twitter_Shared.Data.Model;

namespace Twitter.Controllers
{
    public class LoginController : Controller
    {

        public ActionResult Index()
        {
            return View(new LoginModel());
        }

        [HttpPost, ActionName("RegisterUser")]
        public ActionResult RegisterUser(LoginModel model)
        {
            if ( ModelState.IsValid)
            {
                using (TwitterContext ctx = new TwitterContext())
                {
                    User newUser = new User()
                    {
                        Email = model.NewUser.Email,
                        Location = model.NewUser.Location,
                        Name = model.NewUser.Name,
                        Password = model.NewUser.Password
                    };

                    ctx.Users.Add(newUser);
                    ctx.SaveChanges();
                    FormsAuthentication.SetAuthCookie(model.NewUser.Email, false);

                    return RedirectToAction("Index", "Home");
                }
            } 
            else 
            {
                return RedirectToAction("Index", "Login", model);
            }
        }

        [HttpPost]
        public ActionResult Index(LoginModel model, string returnUrl)
        {
            using (TwitterContext ctx = new TwitterContext())
            {
                if (ctx.Users.Where(u => u.Email == model.EmailAddress && u.Password == model.Password).Count() != 0)
                {
                    FormsAuthentication.SetAuthCookie(model.EmailAddress, false);
                    if (returnUrl != null)
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ModelState.AddModelError("result", "Incorrect user name or password.  Try again.");
                    return View(model);
                }
            }
            return View(model);
        }

    }
}
