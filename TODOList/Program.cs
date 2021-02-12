using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

namespace TODOList
{
    public abstract class TODOListObject
    {
        public TODOListObject()
        {

        }

        public abstract JObject ToJSON();
    }

    public class Task : TODOListObject
    {
        private string task;
        private static int task_count = 0;
        private int id;

        public string TaskDescription
        {
            get { return task; }
        }

        public int ID
        {
            get { return id; }
        }

        public static int TaskCount
        {
            get { return task_count; }
            set { task_count = value; }
        }

        public Task(string task)
        {
            this.task = task;
            id = ++task_count;
        }

        public override JObject ToJSON()
        {
            JObject task = new JObject();
            task.Add("task", this.task);
            return task;
        }
    }

    class Program
    {
        static JObject todo_list;
        static List<Task> tasks = new List<Task>();
        static string user;

        static void Main(string[] args)
        {
            try
            {
                //try to load the to do list
                //if we fail, then we should create the file.
                todo_list = JObject.Parse(File.ReadAllText(Path.Combine(Assembly.GetExecutingAssembly().Location, "todo_list.json")));
            }

            catch
            {
                try
                {
                    todo_list = new JObject();
                    todo_list.Add("user", null);
                    todo_list.Add("tasks", new JArray());

                    using (StreamWriter writer = new StreamWriter(Path.Combine(Assembly.GetExecutingAssembly().Location, "todo_list.json")))
                    {
                        writer.Write(todo_list.ToString());
                    }
                }

                catch
                {
                    //if we fail to create the file, print a message and exit
                    Console.WriteLine("Could not create TODO list.");
                    Exit(1);
                }
            }

            //if we are here we should load the data into our memory locations
            try
            {
                user = todo_list.Value<string>("user");
                foreach (JObject task in todo_list.Value<JArray>("tasks"))
                {
                    tasks.Add(new Task(task.Value<string>("task")));
                }
            }

            catch
            {
                Console.WriteLine("An error occurred when loading the list of tasks.");
                Exit(0);
            }

            //no matter what show the welcome message
            ShowWelcomeMessage();

            //now that we have loaded the file with the todo list, we need to manage a bunch of switches
            if (args.Length == 0)
            {
                //if there are no command line arguments, we should return a message saying to put in the help switch
                Console.WriteLine("No option selected. To see available options, use the --help switch.");
                Exit(0);
            }

            //the first possible switch is the --help switch
            switch(args[0])
            {
                case "--help": ShowPossibleSwitches(); break;
                case "--show": ShowTODOList(); break;
                case "--create": 
                    try
                    {
                        CreateTask(args[1]);
                    }

                    catch
                    {
                        Console.WriteLine("Specify a name for the task. To see available options, use the --help switch.");
                        Exit(0);
                    } break;
                case "--set-user":
                    try
                    {
                        SetUser(args[1]);
                    }

                    catch
                    {
                        Console.WriteLine("Specify a name for the user. To see available options, use the --help switch.");
                        Exit(0);
                    } break;
                case "--delete":
                    try
                    {
                        DeleteTask(Int16.Parse(args[1]));
                    }

                    catch
                    {
                        Console.WriteLine("Specify the ID of the task to delete. To see available options, use the --help switch.");
                        Exit(0);
                    }
                    break;
                case "--clear":
                    try
                    {
                        ClearTasks();
                    }

                    catch
                    {
                        Console.WriteLine("An error occurred while clearing tasks. To see available options, use the --help switch.");
                        Exit(0);
                    }
                    break;
                default: Console.WriteLine("Unknown option. To see available options, use the --help switch.");
                         Exit(0); break;
            }
        }

        private static void SaveTODOList()
        {
            try
            {
                todo_list["user"] = user;
                JArray ta = new JArray();
                foreach (Task t in tasks)
                {
                    ta.Add(t.ToJSON());
                }
                todo_list["tasks"] = ta;

                using (StreamWriter w = new StreamWriter(Path.Combine(Assembly.GetExecutingAssembly().Location, "todo_list.json")))
                {
                    w.Write(todo_list.ToString());
                }

                Console.WriteLine("Changes saved successfully.");
            }

            catch(Exception e)
            {
                Console.WriteLine("The changes could not be saved. " + e.Message);
            }
        }

        private static void SetUser(string user_name)
        {
            user = user_name;
            SaveTODOList();
        }

        private static void CreateTask(string task)
        {
            tasks.Add(new Task(task));
            SaveTODOList();
        }

        private static void DeleteTask(int id)
        {
            Task to_delete = null;

            foreach(Task t in tasks)
            {
                if (t.ID == id)
                {
                    to_delete = t;
                    break;
                }
            }

            if (to_delete != null)
            {
                tasks.Remove(to_delete);
                SaveTODOList();
            }
            else
                Console.WriteLine("The task specified does not exist. Please try again.");
        }

        private static void ClearTasks()
        {
            tasks.Clear();
            SaveTODOList();
        }

        private static void Exit(int code)
        {
            Environment.Exit(code);
        }

        private static void ShowPossibleSwitches()
        {
            Console.WriteLine("The following options are available:");
            Console.WriteLine("--help            \t\t" + "Show available options");
            Console.WriteLine("--show            \t\t" + "Show active tasks");
            Console.WriteLine("--create <task>   \t\t" + "Create a new task");
            Console.WriteLine("--delete <task>   \t\t" + "Delete an existing task");
            Console.WriteLine("--clear           \t\t" + "Delete all existing tasks");
            Console.WriteLine("--set-user <user> \t\t" + "Set the user for the TODO list");
            Exit(0);
        }

        private static void ShowTODOList()
        {
            if (todo_list.Value<JArray>("tasks").Count == 0)
            {
                Console.WriteLine("No Current Tasks.");
                Exit(0);
            }

            Console.WriteLine("Current Tasks");
            Console.WriteLine("-------------");

            foreach (Task task in tasks)
            {
                string spacing = string.Empty;
                for(int i = 0; i < (7 - Math.Floor(Math.Log10(task.ID) + 1)); i++)
                {
                    spacing += " ";
                }
                spacing = task.ID + "." + spacing;
                Console.Write(spacing);
                Console.WriteLine(task.TaskDescription); 
            }
        }

        private static void ShowWelcomeMessage()
        {
            Console.WriteLine("--------------------\n" + "|     TODO List    |\n" + "--------------------\n");

            if(user != null && user != string.Empty)
                Console.WriteLine("Welcome " + user + "!\n");
        }
    }
}
