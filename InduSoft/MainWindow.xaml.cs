using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Input;

namespace InduSoft
{
	public partial class MainWindow : Window
	{
		static string ConnectionStrings = ConfigurationManager.ConnectionStrings["Base"].ConnectionString;
		private ObservableCollection<Employee> employees = new ObservableCollection<Employee>();

		public MainWindow()
		{
			InitializeComponent();
		}

		private void OnKeyDownHandler(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Return)
			{
				GenerationReport();
			}
		}
		private void Button_Click(object sender, RoutedEventArgs e)
		{
			GenerationReport();
		}
		private void GenerationReport()
		{
			employees.Clear();

			var department = DepartmentIdTextBox.Text;
			var percent = PercentTextBox.Text;
			int result;

			if (String.IsNullOrEmpty(department) || String.IsNullOrEmpty(percent))
			{
				MessageBox.Show("Поля \"ID отдела\" и \"Процент повышения ЗП\" не могут быть пустыми.");
				return;
			}

			if (!int.TryParse(department, out result))
			{
				MessageBox.Show("Идентификатор отдела должен быть целым числом");
				return;
			}

			if (!int.TryParse(percent, out result))
			{
				MessageBox.Show("Процент должен быть числом");
				return;
			}

			try
			{
				using (SqlConnection connection = new SqlConnection(ConnectionStrings))
				{
					connection.Open();

					string sqlQuery = $"EXEC[dbo].[UPDATESALARYFORDEPARTMENT] @DepartmentId = {department}, @Percent = {percent}";
					SqlCommand command = new SqlCommand(sqlQuery, connection);
					SqlDataReader reader = command.ExecuteReader();
					while (reader.Read())
					{
						int id = Convert.ToInt32(reader["Id"]);
						int departmentId = Convert.ToInt32(reader["Department_Id"]);
						int? chiefId = Int32.TryParse(reader["Chief_Id"].ToString(), out var tempVal) ? tempVal : (int?)null;
						string name = reader["Name"].ToString();
						decimal oldSalary = Convert.ToDecimal(reader["OldSalary"]);
						decimal newSalary = Convert.ToDecimal(reader["NewSalary"]);

						employees.Add(new Employee()
						{
							Id = id,
							DepartmentId = departmentId,
							ChiefId = chiefId,
							Name = name,
							OldSalary = oldSalary,
							NewSalary = newSalary
						});
					}

					LVEmployees.ItemsSource = employees;

					reader.Close();

					connection.Close();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Произошла ошибка при построении отчета: {ex.Message}");
			}
		}
	}
}
