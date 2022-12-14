using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using TodoListService.Models;

namespace TodoListService.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class TodoListController : Controller
    {
        static ConcurrentBag<TodoItem> todoStore = new ConcurrentBag<TodoItem>();

        private const string _todoListReadScope = "ToDoList.Read";
        private const string _todoListReadWriteScope = "ToDoList.ReadWrite";

        // GET: api/values
        [HttpGet]
        [RequiredScope(
            AcceptedScope = new string[] { _todoListReadScope, _todoListReadWriteScope }
            )]
        public IEnumerable<TodoItem> Get()
        {
            string owner = (User.FindFirst(ClaimTypes.NameIdentifier))?.Value;
            return todoStore.Where(t => t.Owner == owner).ToList();
        }

        // POST api/values
        [HttpPost]
        [RequiredScope(
            AcceptedScope = new string[] { _todoListReadWriteScope }
            )]
        public void Post([FromBody]TodoItem Todo)
        {
            string owner = (User.FindFirst(ClaimTypes.NameIdentifier))?.Value;
            todoStore.Add(new TodoItem { Owner = owner, Title = Todo.Title });
        }
    }
}
