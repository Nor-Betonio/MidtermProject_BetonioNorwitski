using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;



namespace Student
{
    public enum MenuOption
    {
        None = 0,
        RegisterStudent = 1,
        EnrollStudentSubject = 2,
        EnterGrades = 3,
        ShowGradeByStudent = 4,
        Exit = 5
    }
    class Program
    {
        private static List<Student> students = new List<Student>();
        private static List<StudentSubject> enrollments = new List<StudentSubject>();
        private static List<StudentGrades> grades = new List<StudentGrades>();
        private static List<string> gradeHistory = new List<string>();

        static void Main(string[] args)
        {

            while (true)
            {
                Console.WriteLine("MAIN MENU");
                Console.WriteLine("1. Register Student");
                Console.WriteLine("2. Enroll Student Subject");
                Console.WriteLine("3. Enter Grades");
                Console.WriteLine("4. Show Grade By Student");
                Console.WriteLine("5. Exit");
                Console.Write("Select an option (1-5): ");

                string? option = Console.ReadLine();
                option = option ?? string.Empty;

                
                if (!int.TryParse(option.Trim(), out var optNum))
                {
                    Console.WriteLine("Invalid option, try again.");
                    Console.WriteLine();
                    continue;
                }

                var choice = Enum.IsDefined(typeof(MenuOption), optNum) ? (MenuOption)optNum : MenuOption.None;

                if (choice == MenuOption.Exit)
                    break;

                switch (choice)
                {
                    case MenuOption.RegisterStudent:
                        var s = Student.RegisterStudent();
                        students.Add(s);
                        if (PauseAndClear())
                        {
                            students.Clear();
                            enrollments.Clear();
                        }
                        break;
                    case MenuOption.EnrollStudentSubject:
                        if (students.Count == 0)
                        {
                            Console.WriteLine("No students registered. Please register a student first.");
                            break;
                        }

                        Console.WriteLine("Select a student to enroll (enter number) or type a name to search/register:");
                        for (int i = 0; i < students.Count; i++)
                            Console.WriteLine($"{i + 1}. {students[i].FirstName} {students[i].LastName} (Course: {students[i].Course})");

                        Console.Write("Choice (number or name): ");
                        var selInput = Console.ReadLine() ?? string.Empty;

                        Student? selected = null;
                        if (int.TryParse(selInput, out var selIndex) && selIndex >= 1 && selIndex <= students.Count)
                        {
                            selected = students[selIndex - 1];
                        }
                        else
                        {
                            
                            var nameQuery = selInput.Trim();
                            selected = students.Find(s => ($"{s.FirstName} {s.LastName}".Equals(nameQuery, StringComparison.OrdinalIgnoreCase))
                                                           || s.FirstName.Equals(nameQuery, StringComparison.OrdinalIgnoreCase)
                                                           || s.LastName.Equals(nameQuery, StringComparison.OrdinalIgnoreCase));

                            if (selected == null)
                            {
                                Console.Write("Student not found. Register new student now? (y/n): ");
                                var resp = (Console.ReadLine() ?? string.Empty).Trim();
                                if (resp.Equals("y", StringComparison.OrdinalIgnoreCase))
                                {
                                    var newStu = Student.RegisterStudent();
                                    students.Add(newStu);
                                    selected = newStu;
                                }
                                else
                                {
                                    Console.WriteLine("Enrollment cancelled.");
                                    break;
                                }
                            }
                        }

                        if (selected == null)
                        {
                            Console.WriteLine("Enrollment cancelled.");
                            break;
                        }

                        var ss = StudentSubject.EnrollStudent(selected);
                        enrollments.Add(ss);

                        if (PauseAndClear())
                        {
                            students.Clear();
                            enrollments.Clear();
                        }
                        break;
                    case MenuOption.EnterGrades:
                        if (students.Count == 0)
                        {
                            Console.WriteLine("No students registered. Please register first.");
                            break;
                        }

                        Console.WriteLine("Select a student to enter grades for (number or name):");
                        for (int i = 0; i < students.Count; i++)
                            Console.WriteLine($"{i + 1}. {students[i].FirstName} {students[i].LastName}");

                        Console.Write("Choice (number or name): ");
                        var gradeSel = Console.ReadLine() ?? string.Empty;
                        Student? gradeStudent = null;
                        if (int.TryParse(gradeSel, out var gidx) && gidx >= 1 && gidx <= students.Count)
                        {
                            gradeStudent = students[gidx - 1];
                        }
                        else
                        {
                            var nameQuery = gradeSel.Trim();
                            gradeStudent = students.Find(s => $"{s.FirstName} {s.LastName}".Equals(nameQuery, StringComparison.OrdinalIgnoreCase)
                                                           || s.FirstName.Equals(nameQuery, StringComparison.OrdinalIgnoreCase)
                                                           || s.LastName.Equals(nameQuery, StringComparison.OrdinalIgnoreCase));
                            if (gradeStudent == null)
                            {
                                Console.Write("Student not found. Register new student now? (y/n): ");
                                var resp = (Console.ReadLine() ?? string.Empty).Trim();
                                if (resp.Equals("y", StringComparison.OrdinalIgnoreCase))
                                {
                                    var newStu = Student.RegisterStudent();
                                    students.Add(newStu);
                                    gradeStudent = newStu;
                                }
                                else
                                {
                                    Console.WriteLine("Operation cancelled.");
                                    break;
                                }
                            }
                        }

                        var fullNameForEnroll = $"{gradeStudent.FirstName} {gradeStudent.LastName}";
                        var studentEnrolls = enrollments.FindAll(e => e.StudentName.Equals(fullNameForEnroll, StringComparison.OrdinalIgnoreCase));
                        if (studentEnrolls.Count == 0)
                        {
                            Console.WriteLine("This student has no enrolled subjects. Enroll subjects first.");
                            break;
                        }

                        Console.WriteLine("Select subject:");
                        for (int i = 0; i < studentEnrolls.Count; i++)
                            Console.WriteLine($"{i + 1}. {studentEnrolls[i].SubjectName} ({studentEnrolls[i].SubjectCode})");

                        Console.Write("Choice: ");
                        if (!int.TryParse(Console.ReadLine() ?? "", out var subChoice) || subChoice < 1 || subChoice > studentEnrolls.Count)
                        {
                            Console.WriteLine("Invalid choice.");
                            break;
                        }

                        var chosen = studentEnrolls[subChoice - 1];
                        Console.Write("Enter grade: ");
                        if (!int.TryParse(Console.ReadLine() ?? "", out var gradeVal))
                        {
                            Console.WriteLine("Invalid grade.");
                            break;
                        }

                        var newGrade = new StudentGrades
                        {
                            StudentId = 0,
                            StudentName = $"{gradeStudent.FirstName} {gradeStudent.LastName}",
                            SubjectName = chosen.SubjectName,
                            SubjectCode = chosen.SubjectCode,
                            Grade = gradeVal
                        };
                        grades.Add(newGrade);
                        Console.WriteLine("Grade saved.");
                        break;
                    case MenuOption.ShowGradeByStudent:
                        Console.Write("Enter student name to show grades: ");
                        var query = (Console.ReadLine() ?? string.Empty).Trim();

                        if (string.IsNullOrWhiteSpace(query))
                        {
                            Console.WriteLine("No input provided.");
                            break;
                        }

                        var qlower = query.ToLowerInvariant();
                        var matchedStudents = students.FindAll(s =>
                            ($"{s.FirstName} {s.LastName}".ToLowerInvariant().Contains(qlower)) ||
                            s.FirstName.ToLowerInvariant().Contains(qlower) ||
                            s.LastName.ToLowerInvariant().Contains(qlower));

                        if (matchedStudents.Count == 0)
                        {
                            // fallback: search grades by StudentName field
                            var glist = grades.FindAll(g => g.StudentName.Equals(query, StringComparison.OrdinalIgnoreCase));
                            if (glist.Count == 0)
                                Console.WriteLine("No students or grades found matching that name.");
                            else
                                foreach (var item in glist)
                                    Console.WriteLine($"Subject: {item.SubjectName} ({item.SubjectCode}) - Grade: {item.Grade}");
                        }
                        else
                        {
                            foreach (var student in matchedStudents)
                            {
                                var fullname = $"{student.FirstName} {student.LastName}";
                                Console.WriteLine($"Grades for {fullname}:");
                                var glist = grades.FindAll(g => g.StudentName.Equals(fullname, StringComparison.OrdinalIgnoreCase));
                                if (glist.Count == 0)
                                    Console.WriteLine("  No grades recorded for this student.");
                                else
                                    foreach (var item in glist)
                                        Console.WriteLine($"  Subject: {item.SubjectName} ({item.SubjectCode}) - Grade: {item.Grade}");
                            }
                        }

                        break;
                    default:
                        Console.WriteLine("Invalid option, try again.");
                        break;
                }

                Console.WriteLine();
            }

        }

        private static bool PauseAndClear()
        {
            Console.WriteLine("\nPress Enter to return to the main menu. Type 'r' then Enter to reset history and clear screen.");
            var input = Console.ReadLine() ?? string.Empty;
            Console.Clear();
            return input.Equals("r", StringComparison.OrdinalIgnoreCase);
        }
    }

    public class Student
    {
        
        public string FirstName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; } = DateTime.MinValue;

        public int Age { get; set; }

        public string Address { get; set; } = string.Empty;
        public int ContactNumber { get; set; }
        public string Course { get; set; } = string.Empty;

        public int YearLevel { get; set; } = 0;

        public static Student RegisterStudent()
        {
            Console.WriteLine("\n-----STUDENT REGISTRATION-----");

            Console.Write("First Name: ");
            string FirstName = Console.ReadLine() ?? string.Empty;

            Console.Write("Middle Name: ");
            string MiddleName = Console.ReadLine() ?? string.Empty;

            Console.Write("Last Name: ");
            string LastName = Console.ReadLine() ?? string.Empty;

            Console.Write("Date of Birth (yyyy-mm-dd): ");
            string? dobInput = Console.ReadLine();
            DateTime dob;
            if (!DateTime.TryParse(dobInput, out dob))
            {
                dob = DateTime.MinValue;
            }

            Console.WriteLine("Enter Age:");
            int age;
            int.TryParse(Console.ReadLine() ?? "0", out age);

            Console.Write("Address: ");
            string Address = Console.ReadLine() ?? string.Empty;

            Console.WriteLine("Contact Number:");
            int ContactNumber;
            int.TryParse(Console.ReadLine() ?? "0", out ContactNumber);

            Console.WriteLine("Course:");
            string Course = Console.ReadLine() ?? string.Empty;

            Console.WriteLine("Year Level:");
            int YearLevel;
            int.TryParse(Console.ReadLine() ?? "0", out YearLevel);

            
            var student = new Student
            {
                FirstName = FirstName,
                MiddleName = MiddleName,
                LastName = LastName,
                DateOfBirth = dob,
                Age = age,
                Address = Address,
                ContactNumber = ContactNumber,
                Course = Course,
                YearLevel = YearLevel
            };

            Console.WriteLine("\nRegistration Successful!");
            Console.WriteLine($"Name: {LastName} | {MiddleName} | {FirstName} | {Course} | {YearLevel}");

            return student;
        }
    }

    internal class StudentSubject
    {
        public string StudentName { get; set; } = string.Empty;
        public string StudentCourse { get; set; } = string.Empty;
        public int StudentYearLevel { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public string SubjectCode { get; set; } = string.Empty;
        public int Units { get; set; } = 0;

   
        public static StudentSubject EnrollStudent(Student selected)
        {
            Console.WriteLine("\n-----STUDENT SUBJECT ENROLLMENT-----");

            // Use selected student info by default
            var studentName = $"{selected.FirstName} {selected.LastName}";
            var studentCourse = selected.Course;
            var studentYearLevel = selected.YearLevel;

            Console.WriteLine($"Enrolling: {studentName} (Course: {studentCourse}, Year: {studentYearLevel})");
            Console.Write("Press Enter to continue or type 'e' then Enter to edit student info: ");
            var editResp = Console.ReadLine() ?? string.Empty;
            if (editResp.Equals("e", StringComparison.OrdinalIgnoreCase))
            {
                Console.Write("Student Name: ");
                var inputName = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(inputName)) studentName = inputName.Trim();

                Console.Write("Student Course: ");
                var inputCourse = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(inputCourse)) studentCourse = inputCourse.Trim();

                Console.Write("Student Year Level: ");
                if (!int.TryParse(Console.ReadLine() ?? string.Empty, out studentYearLevel))
                    studentYearLevel = selected.YearLevel;
            }

            Console.Write("Subject Name: ");
            var subjectName = Console.ReadLine() ?? string.Empty;

            Console.Write("Subject Code: ");
            var subjectCode = Console.ReadLine() ?? string.Empty;

            Console.Write("Units: ");
            int.TryParse(Console.ReadLine() ?? "0", out var units);

            Console.WriteLine($"\nEnrollment Recorded:");
            Console.WriteLine($"Student: {studentName} | Course: {studentCourse} | Year: {studentYearLevel}");
            Console.WriteLine($"Subject: {subjectName} | Code: {subjectCode} | Units: {units}");

            return new StudentSubject
            {
                StudentName = studentName,
                StudentCourse = studentCourse,
                StudentYearLevel = studentYearLevel,
                SubjectName = subjectName,
                SubjectCode = subjectCode,
                Units = units
            };
        }


    }
    internal class StudentGrades
    {
       
        public int StudentId { get; set; } = 0;
        public string StudentName { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string SubjectCode { get; set; } = string.Empty;
        public int Grade { get; set; }

        public static StudentGrades EnterGrades()
        {
            Console.WriteLine("\n-----ENTER GRADES-----");

            Console.Write("Student ID: ");
            int.TryParse(Console.ReadLine() ?? "0", out var studentId);

           
            Console.Write("Subject Name: ");
            var subjectName = Console.ReadLine() ?? string.Empty;
            Console.Write("Subject Code: ");
            var subjectCode = Console.ReadLine() ?? string.Empty;
            Console.Write("Grade: ");
            int.TryParse(Console.ReadLine() ?? "0", out var grade);
            Console.WriteLine($"\nGrade Recorded:");
            Console.WriteLine($"Student ID: {studentId}");
            Console.WriteLine($"Subject: {subjectName} | Code: {subjectCode} | Grade: {grade}");
            return new StudentGrades
            {
                StudentId = studentId,
                StudentName = string.Empty,
                SubjectName = subjectName,
                SubjectCode = subjectCode,
                Grade = grade
            };
        }
    }
}









