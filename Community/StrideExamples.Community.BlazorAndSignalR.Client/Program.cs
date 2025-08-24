using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.VisualBasic;
using StrideExamples.Community.BlazorAndSignalR.Client.Components;
using StrideExamples.Community.BlazorAndSignalR.Client.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddSignalR();
builder.Services.AddResponseCompression(opts =>
{
  opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(["application/octet-stream"]);
});

var app = builder.Build();
app.UseResponseCompression();

if (!app.Environment.IsDevelopment())
{
  app.UseExceptionHandler("/Error", createScopeForErrors: true);
  app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

app.MapHub<Screen1Hub>(StrideExamples.Community.BlazorAndSignalR.Shared.Core.Constants.HubUrl);