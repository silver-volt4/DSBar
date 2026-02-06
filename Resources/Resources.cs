using System.Reflection;

namespace DSBar
{
    public class Resources
    {
        private static Assembly assembly = Assembly.GetExecutingAssembly();
        public static Icon GetIcon(string resourceName)
        {
            return new Icon(assembly.GetManifestResourceStream(resourceName));
        }
    }
}
