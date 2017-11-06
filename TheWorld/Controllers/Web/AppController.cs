using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TheWorld.ViewModels;
using TheWorld.Services;
using Microsoft.Extensions.Configuration;
using TheWorld.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TheWorld.Controllers.Web
{
    public class AppController : Controller
    {
        private IMailService _mailService;
        private IConfigurationRoot _config;
        private readonly IWorldRepository _repository;
        private ILogger<AppController> _logger;

        public AppController(IMailService mailservice, IConfigurationRoot config, IWorldRepository repository,
            ILogger<AppController> logger)
        {
            _mailService = mailservice;
            _config = config;
            _repository = repository;
            _logger = logger;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
                return View();
        }

        [Authorize]
        public IActionResult Trips()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Contact(ContactViewModel model)
        {

            if (ModelState.IsValid)
            {
                if (model.Email.Contains("aol.com"))
                {
                    ModelState.AddModelError(string.Empty, "We don't support AOL addresses");
                }
                else
                {
                    _mailService.SendMail(_config["MailSettings:ToAddress"], model.Email, "From TheWorld", model.Message);
                    ModelState.Clear();
                    ViewBag.UserMessage = "Message Sent";

                }
               

            }
            

            return View();
        }

        public IActionResult About()
        {
            return View();
        }
    }
}
