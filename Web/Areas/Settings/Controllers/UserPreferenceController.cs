﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Cats.Services.Security;
using Cats.Data.Security;
using Cats.Areas.Settings.Models;

namespace Cats.Areas.Settings.Controllers
{
    public class UserPreferenceController : Controller
    {
        //
        // GET: /Settings/UserPreference/

        private readonly IUserAccountService userService;

        public ActionResult Index()
        {
            //var user = userService.GetUserDetail(HttpContext.User.Identity.Name);
            //var userPreference = new UserPreferenceViewModel
            //{
            //    Language = new List<LanguageCode> {new LanguageCode {Language= user.LanguageCode} },
            //    KeyboardLanguage = new List<Keyboard> { new Keyboard{KeyboardLanguage = user.Keyboard }},
            //    PreferedWeightMeasurement = new List<PreferedWeightMeasurementUnit> {new PreferedWeightMeasurementUnit{ PreferedWeightMeasurement = user.PreferedWeightMeasurment }},
            //    DatePreference = new List<Calendar> {new Calendar{DatePreference = user.DatePreference }},
            //    DefaultTheme = new List<Theme> {new Theme{ DefaultTheme = user.DefaultTheme }}
            //};           
            //return View(userPreference);
            var userPreferences = userService.GetUserPreferences();
            UserPreferenceViewModel userPreferencesModel = new UserPreferenceViewModel();
            foreach (var preferences in userPreferences)
            {
                userPreferencesModel.Language.Add(new LanguageCode { Language = preferences.LanguageCode });
                userPreferencesModel.KeyboardLanguage.Add(new Keyboard { KeyboardLanguage = preferences.Keyboard });
                userPreferencesModel.PreferedWeightMeasurement.Add(new PreferedWeightMeasurementUnit { PreferedWeightMeasurement = preferences.PreferedWeightMeasurment });
                userPreferencesModel.DatePreference.Add(new Calendar { DatePreference = preferences.DatePreference });
                userPreferencesModel.DefaultTheme.Add(new Theme { DefaultTheme = preferences.DefaultTheme });
            }
            ViewData["userPreference"] = userPreferencesModel;
            return View(userPreferencesModel);
        }

        [HttpPost]
        public ActionResult Edit()
        {
            var userPreferences = userService.GetUserPreferences();
            UserPreferenceViewModel userPreferencesModel = new UserPreferenceViewModel();
            foreach (var preferences in userPreferences)
            {
                userPreferencesModel.Language.Add(new LanguageCode { Language = preferences.LanguageCode });
                userPreferencesModel.KeyboardLanguage.Add(new Keyboard { KeyboardLanguage = preferences.Keyboard });
                userPreferencesModel.PreferedWeightMeasurement.Add(new PreferedWeightMeasurementUnit { PreferedWeightMeasurement = preferences.PreferedWeightMeasurment });
                userPreferencesModel.DatePreference.Add(new Calendar { DatePreference = preferences.DatePreference });
                userPreferencesModel.DefaultTheme.Add(new Theme { DefaultTheme = preferences.DefaultTheme });
            }
            ViewData["userPreference"] = userPreferencesModel;
            return View(userPreferencesModel);
        }

        public ActionResult Edit(UserPreferenceViewModel model)
        {
            var user = userService.GetUserDetail(HttpContext.User.Identity.Name);
            user.DefaultTheme = model.DefaultTheme.First().DefaultTheme;
            user.DatePreference = model.DatePreference.First().DatePreference;
            user.Keyboard = model.KeyboardLanguage.First().KeyboardLanguage;
            user.LanguageCode = model.Language.First().Language;
            user.PreferedWeightMeasurment = model.PreferedWeightMeasurement.First().PreferedWeightMeasurement;

            // Edit user preference

            return View(model);
        }

    }
}
