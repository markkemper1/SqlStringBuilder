using System;
using System.Reflection;
using System.Text;

namespace SqlStringBuilder
{
	public static class SQL
	{
		public static StringBuilder SELECT(string literal)
		{
			var sb = new StringBuilder("SELECT ");
			sb.Append(literal);
			return sb;
		}
		
		public static StringBuilder SELECT(this StringBuilder sb, string literal)
		{
			sb.Append("SELECT ");
			sb.Append(literal);
			return sb;
		}

		public static StringBuilder SELECT(object item, string alias = null)
		{
			var sb = new StringBuilder("SELECT ");
			sb.COLUMNS(item, alias);
			return sb;
		}

		public static StringBuilder INSERT(string table, params object[] objects)
		{
			var sb = new StringBuilder("INSERT INTO ");
			sb.Append(table);
			sb.Append(@"
( ");
			sb.AppendItems(x => sb.AppendColumnList(null, x), objects);

			sb.Append(" )");
			sb.Append(@" VALUES
");
			sb.Append("( ");

			sb.AppendItems(x=> sb.PARAMS(x), objects);

			sb.Append(" )");

			return sb;
		}

		public static StringBuilder UPDATE(string table, params object[] objects)
		{
			var sb = new StringBuilder("UPDATE ");
			sb.Append(table);
			sb.Append(@" SET
");

			sb.AppendItems(x=> sb.EQ_PARAMS(x), objects);
			return sb;
		}

		public static StringBuilder UPDATE<T>(T parametes, string tableNameOverride = null)
		{
			var tableName = tableNameOverride ?? typeof (T).Name;
			return UPDATE(tableName, parametes);
		}

		public static StringBuilder DELETE(string table)
		{
			var sb = new StringBuilder("DELETE FROM ");
			sb.Append(table);
			return sb;
		}
	
		public static StringBuilder A(this StringBuilder sb, string literal)
		{
			sb.Append(literal);
			return sb;
		}

		public static StringBuilder SELECT_MORE(this StringBuilder sb, object item, string alias = null)
		{
			sb.Append(", ");
			sb.COLUMNS(item, alias);
			return sb;
		}

		public static StringBuilder WHERE(this StringBuilder sb, string literal = null)
		{
			sb.TrimEnd();
			sb.Append(@"
WHERE ");
			sb.Append(literal);
			return sb;
		}

		public static StringBuilder @PARAM(this StringBuilder sb, string name)
		{
			sb.Append("@");
			sb.Append(name);
			return sb;
		}

		public static StringBuilder @PARAMS(this StringBuilder sb, object item)
		{
			var type = item.GetType();

			ForEachFieldAndProperty(type,
			                        f => { sb.PARAM(f.Name); sb.Append(", "); },
			                        p => { sb.PARAM(p.Name); sb.Append(", "); }
				);

			sb.RemoveTrailing(2);

			return sb;
		}

		public static StringBuilder EQ_PARAM(this StringBuilder sb, string columnName)
		{
			sb.COLUMN(columnName).A(" = ").PARAM(columnName);
			return sb;
		}

		public static StringBuilder EQ_PARAMS(this StringBuilder sb, object item)
		{
			var type = item.GetType();

			ForEachFieldAndProperty(type,
			                        f => sb.EQ_PARAM(f.Name).A(", "),
			                        p => sb.EQ_PARAM(p.Name).A(", ")
				);
			sb.RemoveTrailing(2);

			return sb;
		}

		public static StringBuilder SELECT_IDENTITY(this StringBuilder sb)
		{
			sb.Append(@"
SELECT @@IDENTITY");
			return sb;
		}

		public static StringBuilder SELECT_SCOPE_IDENTITY(this StringBuilder sb)
		{
			sb.Append(@"
SELECT SCOPE_IDENTITY()");
			return sb;
		}

		public static StringBuilder FROM(this StringBuilder sb, string literal)
		{
			sb.Append(@"
FROM ");
			sb.Append(literal);
			return sb;
		}

		public static StringBuilder COLUMNS(this StringBuilder sb, object item, string prefix = null)
		{
			var type = item.GetType();

			ForEachFieldAndProperty(type,
			                        f => sb.COLUMN(f.Name, prefix).A(", "),
			                        p => sb.COLUMN(p.Name, prefix).A(", "));

			sb.RemoveTrailing(2);

			return sb;
		}

		public static StringBuilder COLUMN(this StringBuilder sb, string name, string prefix = null)
		{
			if (!String.IsNullOrEmpty(prefix))
			{
				sb.Append(prefix);
				sb.Append(".");
			}

			sb.Append("[");
			sb.Append(name);
			sb.Append("]");

			return sb;
		}

		internal static void AppendColumnList(this StringBuilder sb, string prefix, object item)
		{
			if (item == null) return;

			var type = item.GetType();

			ForEachFieldAndProperty(type,
			                        f => sb.COLUMN(f.Name, prefix).A(", "),
			                        p => sb.COLUMN(p.Name, prefix).A(", "));

			sb.RemoveTrailing(2);
		}

		internal static void TrimEnd(this StringBuilder sb)
		{
			int i;
			int removed = 0;

			for (i = sb.Length - 1; i >= 0; i--)
			{
				if (!Char.IsWhiteSpace(sb[i]))
					break;
				removed++;
			}

			if (removed == 0) return;

			sb.Remove(i + 1, removed);
		}

		internal static void RemoveTrailing(this StringBuilder sb, int characterCount)
		{
			sb.Remove(sb.Length - characterCount, characterCount);
		}

		public static void AppendItems(this StringBuilder sb, Action<object> objectAppender, params object[] items)
		{
			bool first = true;

			foreach (var item in items)
			{
				if (!first)
					sb.Append(", ");

				if (item is string)
				{
					sb.Append(item);
				}
				else
				{
					objectAppender(item);
				}

				first = false;
			}
		}

		private static void ForEachFieldAndProperty(Type type, Action<FieldInfo> fieldAction, Action<MemberInfo> propertyAction)
		{
			foreach (var prop in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
			{
				fieldAction(prop);
			}

			foreach (var prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
			{
				propertyAction(prop);
			}
		}
	}
}