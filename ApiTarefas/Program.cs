using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata.Ecma335;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase("TarefasDB"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// MapGet são métodos de extensão, estaticos
app.MapGet("/", () => "Olá mundo!");

app.MapGet("frases", async()=> await new HttpClient().GetStreamAsync("https://ron-swanson-quotes.herokuapp.com/v2/quotes"));

// Retorna todas as tarefas
app.MapGet("/tarefas", async(AppDbContext db) => await db.Tarefas.ToListAsync());

// Retorna as tarefas por ID
app.MapGet("/tarefas/{id}", async(int id, AppDbContext db) => 
                await db.Tarefas.FindAsync(id) is Tarefa tarefa ? Results.Ok(tarefa) : Results.NotFound());

// Retorna todas as tarefas que estão marcadas como concluídas
app.MapGet("/tarefas/concluida", async (AppDbContext db) => await db.Tarefas.Where(t => t.IsConcluido).ToListAsync());

// Cria uma nova tarefa
app.MapPost("/tarefas", async (Tarefa tarefa, AppDbContext db) => 
    { 
        db.Tarefas.Add(tarefa); 
        await db.SaveChangesAsync();
        return Results.Created($"/tarefas/{tarefa.Id}", tarefa);
    }
);

app.Run();

class Tarefa 
{
    public int Id { get; set; }
    public string? Nome { get; set; }
    public bool IsConcluido { get; set; }
}

class AppDbContext : DbContext 
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<Tarefa> Tarefas => Set<Tarefa>();
}

