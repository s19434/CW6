using System;
using CW4.Services;
using CW4.DTOs.Requests;
using Microsoft.AspNetCore.Mvc;

namespace CW4.Controllers
{
    [ApiController]
    [Route("api/enrollments")]
    public class EnrollmentsController : ControllerBase
    {
        private readonly IStudentsDbService _service;

        public EnrollmentsController(IStudentsDbService service)
        {
            this._service = service;
        }



        [HttpPost("promotions")]
        public IActionResult PromoteStudent(PromoteStudentRequest request)
        {
            var result = _service.PromoteStudent(request);
            if (result != null)
            {
                ObjectResult res = new ObjectResult(result);
                res.StatusCode = 201;
                return res;
            }
            return NotFound();
        }


        [HttpPost]
        public IActionResult CreateStudent(EnrollStudentRequest request)
        {
            var result = _service.EnrollStudent(request);
            if (result != null) return Ok(result);
            return NotFound();
        }



    }
}
