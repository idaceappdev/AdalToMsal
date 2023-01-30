using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TodoSPA.Models;

namespace TodoSPA.DAL
{
    public class TodoListServiceContext
    {
        public TodoListServiceContext()
           
        { Todoes = new List<Todo>(); }
        public List<Todo> Todoes { get; set; }

        internal void SaveChanges()
        {
            
        }
    }
}
