using System.Globalization;

/// <summary>Stores the identifying and academic information for one student.</summary>
public sealed class Student
{
    /// <summary>Gets the student's full name.</summary>
    public string Name { get; }

    /// <summary>Gets the student's specialty.</summary>
    public string Specialty { get; }

    /// <summary>Gets the student's study group.</summary>
    public string Group { get; }

    /// <summary>Gets the student's average grade.</summary>
    public decimal Grade { get; }

    /// <summary>Creates a student record from validated values.</summary>
    /// <param name="name">The student's full name.</param>
    /// <param name="specialty">The student's specialty.</param>
    /// <param name="group">The student's study group.</param>
    /// <param name="grade">The average grade, from 0 through 100.</param>
    public Student(string name, string specialty, string group, decimal grade)
    {
        Name = name;
        Specialty = specialty;
        Group = group;
        Grade = grade;
    }

    /// <summary>Formats the student fields for display in the console.</summary>
    /// <returns>A readable single-line student summary.</returns>
    public override string ToString() => $"{Name} | {Specialty} | Группа: {Group} | Средний балл: {Grade}";
}

/// <summary>Manages the in-memory collection of student records.</summary>
public sealed class StudentRegistry
{
    private readonly List<Student> students = [];

    /// <summary>Gets all registered students in insertion order.</summary>
    public IReadOnlyList<Student> Students => students.AsReadOnly();

    /// <summary>Finds students whose selected field contains the supplied text, ignoring case.</summary>
    /// <param name="field">A field name: name, specialty, or group.</param>
    /// <param name="query">The text to find.</param>
    /// <returns>Matching students in insertion order; an empty list when no matches exist.</returns>
    /// <exception cref="ArgumentException">The field name is not supported.</exception>
    public IReadOnlyList<Student> Search(string field, string query)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(field);
        ArgumentNullException.ThrowIfNull(query);

        Func<Student, string> selector = field.Trim().ToLowerInvariant() switch
        {
            "name" or "имя" => student => student.Name,
            "specialty" or "специальность" => student => student.Specialty,
            "group" or "группа" => student => student.Group,
            _ => throw new ArgumentException("Поле должно быть name, specialty или group.", nameof(field))
        };

        return students
            .Where(student => selector(student).Contains(query.Trim(), StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }

    /// <summary>Adds a student after validating required text and grade.</summary>
    /// <param name="student">The student record to add.</param>
    /// <exception cref="ArgumentNullException">The student record is null.</exception>
    /// <exception cref="ArgumentException">A text field is empty or the grade is outside 0–100.</exception>
    public void Add(Student student)
    {
        ArgumentNullException.ThrowIfNull(student);
        if (string.IsNullOrWhiteSpace(student.Name) ||
            string.IsNullOrWhiteSpace(student.Specialty) ||
            string.IsNullOrWhiteSpace(student.Group))
            throw new ArgumentException("Имя, специальность и группа обязательны.", nameof(student));
        if (student.Grade is < 0 or > 100)
            throw new ArgumentException("Средний балл должен быть от 0 до 100.", nameof(student));

        students.Add(student);
    }
}

internal static class Program
{
    private static void Main()
    {
        var registry = new StudentRegistry();
        Console.WriteLine("Учебный журнал студентов");
        Console.WriteLine("Для завершения оставьте имя пустым.");

        while (true)
        {
            Console.Write("Имя студента: ");
            string name = (Console.ReadLine() ?? "").Trim();
            if (name.Length == 0)
                break;

            Console.Write("Специальность: ");
            string specialty = (Console.ReadLine() ?? "").Trim();
            Console.Write("Группа: ");
            string group = (Console.ReadLine() ?? "").Trim();
            Console.Write("Средний балл (0–100): ");
            string gradeText = (Console.ReadLine() ?? "").Trim().Replace(',', '.');

            if (!decimal.TryParse(gradeText, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal grade))
            {
                Console.WriteLine("Ошибка: введите числовой средний балл.");
                continue;
            }

            try
            {
                registry.Add(new Student(name, specialty, group, grade));
                Console.WriteLine("Студент добавлен.");
            }
            catch (ArgumentException exception)
            {
                Console.WriteLine($"Ошибка: {exception.Message}");
            }
        }

        Console.WriteLine("\nСписок студентов:");
        if (registry.Students.Count == 0)
            Console.WriteLine("Список пуст.");
        foreach (Student student in registry.Students)
            Console.WriteLine(student);

        if (registry.Students.Count > 0)
        {
            Console.Write("\nПоиск: поле (имя, специальность, группа): ");
            string field = (Console.ReadLine() ?? "").Trim();
            Console.Write("Текст для поиска: ");
            string query = (Console.ReadLine() ?? "").Trim();
            try
            {
                IReadOnlyList<Student> matches = registry.Search(field, query);
                Console.WriteLine(matches.Count == 0 ? "Совпадений не найдено." : "Результаты поиска:");
                foreach (Student student in matches)
                    Console.WriteLine(student);
            }
            catch (ArgumentException exception)
            {
                Console.WriteLine($"Ошибка: {exception.Message}");
            }
        }
    }
}

