﻿using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using loginregistration.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;

namespace loginregistration.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private MyContext _context; 
    public HomeController(ILogger<HomeController> logger, MyContext context)
    {
        _logger = logger;
        _context = context;    
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

[HttpPost("user/create")]
    public IActionResult CreateUser(Usuario _user){
        if (ModelState.IsValid){
            PasswordHasher<Usuario> Hasher = new PasswordHasher<Usuario>();   
            _user.Password = Hasher.HashPassword(_user, _user.Password); 
            _context.Add(_user);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }else{
            return RedirectToAction("Index");
        }
    }

 //LOGIN ROUTES
    [HttpPost("/login")]
    public IActionResult Login(LoginUser userSubmission) //parameter to pass in, the user submission of their email and password
    {
        //EMAIL CHECK
        //Check if submission is valid according to our model
        if (!ModelState.IsValid) //If submission is not valid according to our model
        {
            return View("Index"); //return them back to the index view to register/login again
        }

        //Check if email submitted is in our database
        Usuario? userInDb = _context.Usuarios.FirstOrDefault(e => e.Email == userSubmission.Email);

        //If nothing comes back, add an error and return the same view to render the validation
        if (userInDb == null)
        {
            ModelState.AddModelError("Email", "Invalid Email Address/Password");
            return View("Index");
        }

        //PASSWORD CHECK
        //Invoke password hasher
        PasswordHasher<LoginUser> hashbrowns = new PasswordHasher<LoginUser>();
        //LoginUser, hashed password, password submitted by user
        var result = hashbrowns.VerifyHashedPassword(userSubmission, userInDb.Password, userSubmission.Password);
        

        if (result == 0)
        {
            ModelState.AddModelError("Email", "Invalid Email Address/Password"); //string key, error message parameters
            return View("Index");
        }

        //PASSES ALL CHECKS, HANDLE SUCCESS NOW
        //We want to set the session key:value pair to id:UserId and we'll use this to keep track if our user is logged in
        HttpContext.Session.SetInt32("UUID", userInDb.UsuarioId);
        return View("Sucess");
    }

   
    // Name this anything you want with the word "Attribute" at the end
public class SessionCheckAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        // Find the session, but remember it may be null so we need int?
        int? userId = context.HttpContext.Session.GetInt32("UserId");
        // Check to see if we got back null
        if(userId == null)
        {
            // Redirect to the Index page if there was nothing in session
            // "Home" here is referring to "HomeController", you can use any controller that is appropriate here
            context.Result = new RedirectToActionResult("Index", "Home", null);
        }
    }
}

  [HttpGet("/sucess")]
    [SessionCheck]
    public IActionResult Sucess()
    {

        return View();
    }

    public IActionResult Logout(){

        HttpContext.Session.Clear();
        return RedirectToAction("Index");
    }
    }
