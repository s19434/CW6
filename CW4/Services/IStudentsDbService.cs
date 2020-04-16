using System;
using System.Collections.Generic;
using CW4.DTOs.Requests;
using CW4.Models;

namespace CW4.Services
{
    public interface IStudentsDbService
    {
        public bool MidIfIndexExist(string index);
        public IEnumerable<Student> GetStudents();
        public IEnumerable<Enrollment> GetEnrollments(string index);
        public Enrollment EnrollStudent(EnrollStudentRequest request);
        public Enrollment PromoteStudent(PromoteStudentRequest request);
    }
}
