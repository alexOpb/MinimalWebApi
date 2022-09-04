using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MinimalWebApi;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
services.AddDatabaseDeveloperPageExceptionFilter();

var app = builder.Build();

app.MapGet("/", () => "Hello World");

app.MapGet("/todoitems", async (TodoDb db) => 
    await db.Todos.Select(x => new TodoDTO(x)).ToListAsync());
app.MapGet("/todoitems/complete", async (TodoDb db) => 
    await db.Todos.Where(t => t.IsComplete).Select(x => new TodoDTO(x)).ToListAsync());
app.MapGet("/todoitems/{id}", async (int id, TodoDb db) =>
    await db.Todos.FindAsync(id)
        is Todo todo
        ? Results.Ok(new TodoDTO(todo))
        : Results.NotFound());

app.MapPost("/todoitems", async (TodoDTO todoDTO, TodoDb db) =>
{
    var todo = new Todo
    {
        Name = todoDTO.Name,
        IsComplete = todoDTO.IsComplete,
    };
    
    db.Todos.Add(todo);
    await db.SaveChangesAsync();

    return Results.Created($"/todoitems/{todo.Id}", new TodoDTO(todo));
});

app.MapPut("/todoitems/{id}", async (int id, TodoDTO inputTodoDTO, TodoDb db) =>
{
    
    var todo = await db.Todos.FindAsync(id);

    if (todo is null) return Results.NotFound();

    todo.Name = inputTodoDTO.Name;
    todo.IsComplete = inputTodoDTO.IsComplete;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/todoitems/{id}", async (int id, TodoDb db) =>
{
    if (await db.Todos.FindAsync(id) is Todo todo)
    {
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        return Results.Ok(new TodoDTO(todo));
    }

    return Results.NotFound();
});

app.Run();