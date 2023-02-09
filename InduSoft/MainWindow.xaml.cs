using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace InduSoft
{
	public partial class MainWindow : Window
	{
		static readonly string _connectionStrings = ConfigurationManager.ConnectionStrings["Base"].ConnectionString;
		private readonly ObservableCollection<Employee> _employees = new ObservableCollection<Employee>();

		public MainWindow()
		{
			InitializeComponent();
			UpdateItems();
		}

		private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			_ = DepartmentIdComboBox.SelectedItem as Employee;
		}

		private void UpdateItems()
		{
			try
			{
				using (SqlConnection connection = new SqlConnection(_connectionStrings))
				{
					connection.Open();

					SqlCommand sqlCommand = new SqlCommand("SELECT Id, Name FROM Department", connection);
					SqlDataAdapter sqlAdapter = new SqlDataAdapter(sqlCommand);
					DataTable dataTable = new DataTable();
					sqlAdapter.Fill(dataTable);
					DepartmentIdComboBox.ItemsSource = dataTable.DefaultView;
					DepartmentIdComboBox.DisplayMemberPath = "Name";
					DepartmentIdComboBox.SelectedValuePath = "Id";
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Произошла ошибка при построении отчета: {ex.Message}");
			}
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
			_employees.Clear();

			var percent = PercentTextBox.Text;

			if (string.IsNullOrEmpty(DepartmentIdComboBox.Text) || string.IsNullOrEmpty(percent))
			{
				MessageBox.Show("Поля \"ID отдела\" и \"Процент повышения ЗП\" не могут быть пустыми.");
				return;
			}
			
			if (!int.TryParse(percent, out _))
			{
				MessageBox.Show("Процент должен быть числом");
				return;
			}

			if(int.Parse(percent) < 0 || int.Parse(percent) > 100)
			{
				MessageBox.Show("Процент должен быть от 0 до 100");
				return;
			}

			try
			{
				using (SqlConnection connection = new SqlConnection(_connectionStrings))
				{
					connection.Open();

					string sqlQuery = $"EXEC[dbo].[UPDATESALARYFORDEPARTMENT] @DepartmentId = {DepartmentIdComboBox.SelectedValue}, @Percent = {percent}";
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

						_employees.Add(new Employee()
						{
							Id = id,
							DepartmentId = departmentId,
							ChiefId = chiefId,
							Name = name,
							OldSalary = oldSalary,
							NewSalary = newSalary
						});
					}

					LVEmployees.ItemsSource = _employees;

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
