using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using BlissApi.Models;
using System.Net.Mail;
using Newtonsoft.Json.Linq;

namespace BlissApi.Controllers
{
    public class questionsController : ApiController
    {
        private blissdbEntities db = new blissdbEntities();

        [Route("api/share")]
        [HttpGet]
        public HttpResponseMessage share(string email, string conturl)
        {

            try
            {
                MailAddress from = new MailAddress("luismoreira.pda@gmail.com");
                MailAddress to = new MailAddress(email);
                MailMessage mail = new MailMessage();
                mail.To.Add(to);

                mail.From = from;
                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 465;
                smtp.Credentials = new NetworkCredential(
                    "luismoreira.pda@gmail.com", "password");
                smtp.EnableSsl = true;
                mail.Subject = "Share the content of screen";
                mail.Body = conturl;

                smtp.Send(mail);
                return Request.CreateErrorResponse(HttpStatusCode.OK, "OK");

            }
            catch (Exception)
            {

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, " Bad Request. Either destination_email not valid or empty content_url");
            }


        }
        [Route("api/health")]
        [HttpGet]
        public HttpResponseMessage health()
        {

            try
            {
                var exists = db.Database.Exists();


                return Request.CreateErrorResponse(HttpStatusCode.OK, "OK");
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.ServiceUnavailable, "Service Unavailable. Please try again later.");
            }
        }

        // GET: api/questions
        public IQueryable<questions> Getquestions()
        {
            return db.questions.Include(c => c.Choices);
        }

        public IQueryable<questions> Getquestions(int offset, int limit,string filter)
        {
            var questions2 = from q in db.questions
                             select q;

            var choices2 = from c in db.Choices
                           select c;
            if (!String.IsNullOrEmpty(filter))
            {
                questions2 = questions2.Where(s => s.question.Contains(filter)).Include(s=>s.Choices);
                choices2 = choices2.Where(f => f.choice.Contains(filter));
                                       
            }
            var questionsresult = (from q in db.questions
                                   join c in choices2
                                   on q.ID equals c.questionsID
                                   select q).Union(from r in questions2 select r);
                                          

            return questionsresult.Include(c => c.Choices).Skip(offset).Take(limit);
           
        }

        // GET: api/questions/5
        [ResponseType(typeof(questions))]
        public IHttpActionResult Getquestions(int id)
        {
            questions questions = db.questions.Find(id);
            if (questions == null)
            {
                return NotFound();
            }

            return Ok(questions);
        }

        // PUT: api/questions/5
        [ResponseType(typeof(void))]
        //public IHttpActionResult Putquestions(int id, questions questions)
        public IHttpActionResult Putquestions(int id, JObject jsonData)
        {
            dynamic json = jsonData;
            JObject jquestions = json.questions;
            JObject jchoices = json.Choices;

            var questions2 = jquestions.ToObject<questions>();
            List<Choices> choices = jchoices.ToObject<List<Choices>>();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != questions2.ID)
            {
                return BadRequest();
            }

            for (int i = 0; i < choices.Count(); i++)
            {
                db.Entry(choices[i]).State = EntityState.Modified;
            }

            db.Entry(questions2).State = EntityState.Modified;
            

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!questionsExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/questions
        [ResponseType(typeof(questions))]
        //public IHttpActionResult Postquestions(questions questions)
        public IHttpActionResult Postquestions(JObject jsonData)
        {
            dynamic json = jsonData;
            JObject jquestions = json.questions;
            JObject jchoices = json.Choices;

            var questions2 = jquestions.ToObject<questions>();
            List<Choices> choices = jchoices.ToObject<List<Choices>>();

            

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            for (int i = 0; i < choices.Count(); i++)
            {
                if (choices[i].choice == null || choices[0].votes == 0)
                {
                    return BadRequest("Bad Request. All fields are mandatory.");
                }
            }
            for (int i = 0; i < choices.Count(); i++)
            {
                db.Choices.Add(choices[i]);
            }
            db.SaveChanges();
            db.questions.Add(questions2);
            
           
            
            
            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                if (questionsExists(questions2.ID))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = questions2.ID }, questions2);
        }

        // DELETE: api/questions/5
        [ResponseType(typeof(questions))]
        public IHttpActionResult Deletequestions(int id)
        {
            questions questions = db.questions.Find(id);
            if (questions == null)
            {
                return NotFound();
            }

            db.questions.Remove(questions);
            db.SaveChanges();

            return Ok(questions);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool questionsExists(int id)
        {
            return db.questions.Count(e => e.ID == id) > 0;
        }
    }
}