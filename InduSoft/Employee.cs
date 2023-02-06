namespace InduSoft
{
	internal class Employee
	{
		public int Id { get; set; }
		public int DepartmentId { get; set; }
		public int? ChiefId { get; set; }
		public string Name { get; set; }
		public decimal OldSalary { get; set; }
		public decimal NewSalary { get; set; }
	}
}
