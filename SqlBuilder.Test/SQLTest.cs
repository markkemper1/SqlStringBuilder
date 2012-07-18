using System;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace SqlStringBuilder.Test
{
	[TestFixture]
	public class SQLTest
	{
		public class Select
		{
			/* Insert into applications (x x ) values (1,2,3)
			 * SQL.Insert("Applications", blog, new {Id= Guid.New})
			 *		.ToString()
			 * 
			 * Update applications set X = 1, y = 2 where Id = 1
			 * SQL.Update("Applications", blog, new { DateUpdated = DateTime.Now} )
			 *		.Where("Id = @Id").
			 * * 
			 * Delete from Application where Id = 1
			 * SQL.Delete("Applications").Where("[Id] = @Id");
			 *
			 * Select * from Applications where Id = 1
			 * SQL.Select(blog)
			 *	.FROM;
			 */

			[Test]
			public void inset_should_generate_field_name_and_values_for_all_objects_passed()
			{
				AssertEqual(
					@"INSERT INTO TestTab2
( [IamAField], [Integer], [select], [FirstName], [NoGet], [NoSet], [Id], [Test1] ) VALUES
( @IamAField, @Integer, @select, @FirstName, @NoGet, @NoSet, @Id, @Test1 )",
					SQL.INSERT("TestTab2", new select_property_object_example(), new { Id = 1 }, new { Test1 = 2 }));
			}

			[Test]
			public void insert_should_allow_scope_identity_etc_to_be_added_to_end_of_query()
			{
				AssertEqual(@"INSERT INTO TestTab2
( [Id] ) VALUES
( @Id )
SELECT @@IDENTITY", SQL.INSERT("TestTab2", new { Id = 1 }).SELECT_IDENTITY());
			}

			[Test]
			public void update_should_generate_field_name_eq_parameter_for_all_objects_passed()
			{
				AssertEqual(
					@"UPDATE TestTab2 SET
[IamAField] = @IamAField, [Integer] = @Integer, [select] = @select, [FirstName] = @FirstName, [NoGet] = @NoGet, [NoSet] = @NoSet, [Id] = @Id, [Test1] = @Test1"
, SQL.UPDATE("TestTab2", new select_property_object_example(), new { Id = 1 }, new { Test1 = 2 }));
			}

			[Test]
			public void update_should_allow_where_clause()
			{
				AssertEqual(@"UPDATE TestTab2 SET
[Test1] = @Test1
WHERE [Id] = @Id"
, SQL.UPDATE("TestTab2", new { Test1 = 2 })
		.WHERE("[Id] = @Id"));
			}

			[Test]
			public void delete_should_append_table()
			{
				AssertEqual("DELETE FROM testTable", SQL.DELETE("testTable"));
			}

			[Test]
			public void delete_should_return_a_string_builder()
			{
				Assert.IsInstanceOf<StringBuilder>(SQL.DELETE("testTable"));
			}

			[Test]
			public void select_shoud_generate_select_star()
			{
				AssertEqual("SELECT *", SQL.SELECT("*"));
			}

			[Test]
			public void select_with_string_parameter_should_generate_select_with_parameter_literal()
			{
				AssertEqual("SELECT some stuff", SQL.SELECT("some stuff"));
			}

			[Test]
			public void select_with_object_parameter_should_generate_select_with_public_properties_and_fields_as_columns()
			{
				AssertEqual(@"SELECT [IamAField], [Integer], [select], [FirstName], [NoGet], [NoSet]",
							SQL.SELECT(new select_property_object_example()));
			}

			[Test]
			public void select_with_object_and_alias_parameter_should_generate_select_with_aliased_columns()
			{
				AssertEqual(@"SELECT [j].[IamAField], [j].[Integer], [j].[select], [j].[FirstName], [j].[NoGet], [j].[NoSet]",
							SQL.SELECT(new select_property_object_example(), "[j]"));
			}

			[Test]
			public void select_with_literal_object_and_anonymous_should_generate_select_with_combined_fields()
			{
				AssertEqual(@"SELECT some_literal_text, [IamAField], [Integer], [select], [FirstName], [NoGet], [NoSet], [anony1], [anony2]",
							SQL.SELECT("some_literal_text")
								.SELECT_MORE(new select_property_object_example())
								.SELECT_MORE(new { anony1 = 1, anony2 = 3 })
							);
			}

			[Test]
			public void select_with_anonymous_object_parameter_should_generate_select_with_properties_as_columns()
			{
				AssertEqual(@"SELECT [left], [right]", SQL.SELECT(new { left = "1", right = 2 }));
			}

			[Test]
			public void from_alias_should_add_from_and_then_literal()
			{
				AssertEqual(
@"SELECT *
FROM [TestTable] [j]", SQL.SELECT("*").FROM("[TestTable] [j]"));
			}

			[Test]
			public void from_should_return_string_builder_and_allow_where_extension()
			{
				AssertEqual(
@"SELECT *
FROM [TestTable] [j]
JOIN [TestTable2] [t2]
WHERE Id = 1", SQL
				.SELECT("*")
				.FROM(@"[TestTable] [j]
JOIN [TestTable2] [t2]")
				.WHERE("Id = 1")
			 );
			}

			private class select_property_object_example
			{
				public string IamAField = "yea";
				public const string NoDice = "7";
				public static int StaticDude = 12;
				public int Integer { get; set; }
				private string privateProp { get; set; }
				protected int protectedProp { get; set; }
				public string select { get; set; }
				public string FirstName { get; set; }

				public string NoGet
				{
					set { }
				}

				public string NoSet
				{
					get { return ""; }
				}

				public static string NoStaticsShould_be_used { get; set; }
			}
		}

		public static void AssertEqual(string arg1, object arg2)
		{
			Assert.AreEqual(arg1, arg2.ToString());
		}
	}
}
