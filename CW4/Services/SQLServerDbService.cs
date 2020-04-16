using System;
using System.Collections.Generic;
using CW4.Models;
using System.Data.SqlClient;
using CW4.DTOs.Requests;
using Microsoft.VisualBasic.CompilerServices;
using System.Globalization;

namespace CW4.Services
{
    public class SQLServerDbService : IStudentsDbService

    {
        private const string connectionString = "Data Source=db-mssql16.pjwstk.edu.pl;Initial Catalog=s19434;Integrated Security=True";

        public Enrollment PromoteStudent(PromoteStudentRequest studentRequest)
        {
            Enrollment enrollment = null;
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var commands = new SqlCommand())
                {
                    commands.Connection = connection;
                    commands.CommandText = $"Select * From Enrollment e, Studies s Where e.IdStudy = s.IdStudy AND e.Semester = @Semester AND s.Name = @Studies";
                    commands.Parameters.AddWithValue("Studies", studentRequest.Studies);
                    commands.Parameters.AddWithValue("Semester", studentRequest.Semester);

                    var dr = commands.ExecuteReader();

                    if (dr.Read())
                    {
                        enrollment = new Enrollment()
                        {
                            IdEnrollment = Int32.Parse(dr["IdEnrollment"].ToString()),
                            Semester = Int32.Parse(dr["Semester"].ToString()),
                            IdStudy = Int32.Parse(dr["IdStudy"].ToString()),
                            StartDate = dr["StartDate"].ToString()
                        };
                    }
                    dr.Close();
                }


                if (enrollment == null)
                    return null;

                using (var commands = new SqlCommand())
                {
                    commands.Connection = connection;
                    commands.CommandText = "PromoteStudent";
                    commands.CommandType = System.Data.CommandType.StoredProcedure;
                    commands.Parameters.AddWithValue("Studies", studentRequest.Studies);
                    commands.Parameters.AddWithValue("Semester", studentRequest.Semester);

                    var dr = commands.ExecuteReader();
                    if (dr.Read())
                    {
                        enrollment.IdEnrollment = Int32.Parse(dr["IdEnrollment"].ToString());
                        enrollment.Semester = Int32.Parse(dr["Semester"].ToString());
                        enrollment.IdStudy = Int32.Parse(dr["IdStudy"].ToString());
                        enrollment.StartDate = dr["StartDate"].ToString();
                    }
                }
            }

            return enrollment;
        }

   
     

        public IEnumerable<Enrollment> GetEnrollments(string index)
        {
            var enrollments = new List<Enrollment>();
            using (var connection = new SqlConnection(connectionString))
            using (var commands = new SqlCommand())
            {
                commands.Connection = connection;
                commands.CommandText = $"Select * From Enrollment e inner join Student s on e.IdEnrollment = s.IdEnrollment where s.IndexNumber = @index;";
                commands.Parameters.AddWithValue("index", index);

                connection.Open();
                var dr = commands.ExecuteReader();

                while (dr.Read())

                 {
                    var enrollment = new Enrollment();
                    enrollment.IdEnrollment = Int32.Parse(dr["IdEnrollment"].ToString());
                    enrollment.Semester = Int32.Parse(dr["Semester"].ToString());
                    enrollment.IdStudy = Int32.Parse(dr["IdStudy"].ToString());
                    enrollment.StartDate = dr["StartDate"].ToString();

                    enrollments.Add(enrollment);
                }
            }

            return enrollments;
        }

        public Enrollment EnrollStudent(EnrollStudentRequest request)
        {
            
            if (request.IndexNumber == null || request.FirstName == null || request.LastName == null || request.BirthDate == null || request.Studies == null)
                return null;

            Enrollment enrollment = null;
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var transaction = connection.BeginTransaction();


                Studies studies = null;
                using (var commands = new SqlCommand())
                {
                    commands.Connection = connection;
                    commands.Transaction = transaction;
                    commands.CommandText = $"Select * From Studies Where Name = @StudiesName;";
                    commands.Parameters.AddWithValue("StudiesName", request.Studies);

                    var dr = commands.ExecuteReader();

                    if (dr.Read())
                    {
                        studies = new Studies();
                        studies.IdStudy = IntegerType.FromObject(dr["IdStudy"]);
                        studies.Name = dr["Name"].ToString();
                    }
                    dr.Close();
                }

                if (studies == null)
                    return null;

                int index = 0;
                using (var commands = new SqlCommand())
                {
                    commands.Connection = connection;
                    commands.Transaction = transaction;
                    commands.CommandText = $"Select distinct s.IdStudy from Studies s, Enrollment e where s.IdStudy = e.IdStudy AND s.Name = @StudiesName;";
                    commands.Parameters.AddWithValue("StudiesName", request.Studies);

                    var dr = commands.ExecuteReader();

                    if (dr.Read())
                    {
                        index = Int32.Parse(dr["IdStudy"].ToString());
                    }
                    dr.Close();
                }
                
                int idStudy = index;
                
                using (var commands = new SqlCommand())
                {
                    commands.Connection = connection;
                    commands.Transaction = transaction;
                    commands.CommandText = $"Select TOP 1 * From Enrollment Where IdStudy = @idStudy AND Semester = 1 order by StartDate Desc;";
                    commands.Parameters.AddWithValue("IdStudy", idStudy);

                    var dr = commands.ExecuteReader();

                    if (dr.Read())
                    {
                        enrollment = new Enrollment()
                        {
                            IdEnrollment = Int32.Parse(dr["IdEnrollment"].ToString()),
                            Semester = Int32.Parse(dr["Semester"].ToString()),
                            IdStudy = Int32.Parse(dr["IdStudy"].ToString()),
                            StartDate = dr["StartDate"].ToString()
                        };
                    }
                    dr.Close();
                }


                if (enrollment == null)
                {
                    enrollment = new Enrollment();
                    enrollment.Semester = 1;
                    enrollment.IdStudy = idStudy;
                    enrollment.StartDate = DateTime.Now.ToString("dd.MM.yyyy");
                   
                    using (var commands = new SqlCommand())
                    {
                        string strDateFormat = "dd.MM.yyyy";
                        DateTime StartDate = DateTime.ParseExact(enrollment.StartDate, strDateFormat, CultureInfo.InvariantCulture);

                        commands.Connection = connection;
                        commands.Transaction = transaction;

                        commands.CommandText = $"INSERT INTO Enrollment VALUES(SCOPE_IDENTITY(), @Semester, @IdStudy, @StartDate);";
                        commands.Parameters.AddWithValue("Semester", enrollment.Semester);
                        commands.Parameters.AddWithValue("IdStudy", enrollment.IdStudy);
                        commands.Parameters.AddWithValue("StartDate", StartDate);
                        commands.ExecuteNonQuery();
                    }
                }

                bool ifExist = false;

                using (var commands = new SqlCommand())
                {
                    commands.Connection = connection;
                    commands.Transaction = transaction;
                    commands.CommandText = $"SELECT * FROM Student WHERE IndexNumber = @index;";
                    commands.Parameters.AddWithValue("index", request.IndexNumber);

                    var dr = commands.ExecuteReader();
                    ifExist = dr.Read();
                    dr.Close();
                }
                if (ifExist)
                {
                    transaction.Rollback();
                    return null;
                }

                using (var commands = new SqlCommand())
                {
                    string strDateFormat = "dd.MM.yyyy";
                    DateTime BirthDate = DateTime.ParseExact(request.BirthDate.ToString(), strDateFormat, CultureInfo.InvariantCulture);

                    commands.Connection = connection;
                    commands.Transaction = transaction;

                    commands.CommandText = $"INSERT INTO Student VALUES (@IndexNumber, @FirstName, @LastName, @BirthDate, @IdEnrollment)";
                    commands.Parameters.AddWithValue("IndexNumber", request.IndexNumber);
                    commands.Parameters.AddWithValue("FirstName", request.FirstName);
                    commands.Parameters.AddWithValue("LastName", request.LastName);
                    commands.Parameters.AddWithValue("BirthDate", BirthDate);
                    commands.Parameters.AddWithValue("IdEnrollment", enrollment.IdEnrollment);
                    commands.ExecuteNonQuery();
                }
                transaction.Commit();
            }

            return enrollment;
        }

        public IEnumerable<Student> GetStudents()
        {
            List<Student> students = new List<Student>();
            using (var connection = new SqlConnection(connectionString))
            using (var commands = new SqlCommand())
            {
                commands.Connection = connection;
                commands.CommandText = "Select * From Student stu inner join Enrollment enr on stu.IdEnrollment = enr.IdEnrollment " +
                                       "inner join Studies s on s.IdStudy = enr.IdStudy;";

                connection.Open();
                var dr = commands.ExecuteReader();


                while (dr.Read())
                {
                    var st = new Student();
                    st.IndexNumber = dr["IndexNumber"].ToString();
                    st.FirstName = dr["FirstName"].ToString();
                    st.LastName = dr["LastName"].ToString();
                    st.BirthDate = dr["BirthDate"].ToString();
                    st.IdEnrollment = Int32.Parse(dr["IdEnrollment"].ToString());

                    students.Add(st);
                }
            }

            return students;
        }
        public bool MidIfIndexExist(string index)
        {
            bool indexExist = false;

            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            using (SqlCommand sqlCommand = new SqlCommand())
            {

                sqlCommand.Connection = sqlConnection;
                sqlCommand.CommandText = $"Select * From Student st Where st.IndexNumber = @index;";
               
                sqlCommand.Parameters.AddWithValue("index", index);

                sqlConnection.Open();
                var dataReader = sqlCommand.ExecuteReader();
               
                indexExist = dataReader.Read();
                dataReader.Close();
            }


            return indexExist;
        }
    }

}