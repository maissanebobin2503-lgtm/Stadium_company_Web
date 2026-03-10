using Stadium_company;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

var db = DBConnection.Instance();

db.Server = "127.0.0.1";
db.DatabaseName = "Stadium_Questionnaire";
db.UserName = "maissane";
db.Password = Crypto.Decrypt("qm2/U3I+qHRzTTitsWo7BQ==");


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Questionnaire}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
