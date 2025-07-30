using Mendix.StudioPro.ExtensionsAPI.UI.DockablePane;
using Mendix.StudioPro.ExtensionsAPI.UI.WebView;

using System.Diagnostics;

using System.Text.Json;

namespace CommitGraph
{
    public class GitGraphViewModel : WebViewDockablePaneViewModel
    {
        public override void InitWebView(IWebView webView)
        {
            try
            {
                string assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string[] pathParts = assemblyPath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                string projectName = "Unknown Project";
                string projectPath = "Unknown Path";

                for (int i = 0; i < pathParts.Length; i++)
                {
                    if (pathParts[i] == ".mendix-cache" && i > 0)
                    {
                        projectPath = string.Join(Path.DirectorySeparatorChar.ToString(), pathParts, 0, i);
                        projectName = pathParts[i - 1];
                        break;
                    }
                }

                List<Dictionary<string, string>> commits = new List<Dictionary<string, string>>();

                try
                {
                    if (Directory.Exists(Path.Combine(projectPath, ".git")))
                    {
                        var logProcess = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = "git",
                                Arguments = "log --all --decorate --pretty=format:\"%H|%s|%an|%ae|%ad|%P\" --date=iso",
                                WorkingDirectory = projectPath,
                                RedirectStandardOutput = true,
                                UseShellExecute = false,
                                CreateNoWindow = true
                            }
                        };

                        logProcess.Start();
                        string output = logProcess.StandardOutput.ReadToEnd();
                        logProcess.WaitForExit();

                        string[] lines = output.Split('\n');
                        foreach (string line in lines)
                        {
                            if (!string.IsNullOrWhiteSpace(line))
                            {
                                string[] parts = line.Trim().Split('|');
                                if (parts.Length >= 6 &&
                                    !parts[1].Contains("git_note_create") &&
                                    !parts[1].Contains("git notes add") &&
                                    !parts[2].Contains("Team Server"))
                                {
                                    string hash = parts[0];
                                    string branchName = "unknown";

                                    try
                                    {
                                        var branchProcess = new Process
                                        {
                                            StartInfo = new ProcessStartInfo
                                            {
                                                FileName = "git",
                                                Arguments = $"name-rev --name-only {hash}",
                                                WorkingDirectory = projectPath,
                                                RedirectStandardOutput = true,
                                                UseShellExecute = false,
                                                CreateNoWindow = true
                                            }
                                        };

                                        branchProcess.Start();
                                        branchName = branchProcess.StandardOutput.ReadToEnd().Trim();
                                        branchProcess.WaitForExit();
                                    }
                                    catch { }

                                    commits.Add(new Dictionary<string, string>
                                    {
                                        ["hash"] = parts[0],
                                        ["message"] = parts[1],
                                        ["author"] = parts[2],
                                        ["email"] = parts[3],
                                        ["date"] = parts[4],
                                        ["parent"] = parts[5],
                                        ["branch"] = branchName
                                    });
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log the error
                    File.WriteAllText(Path.Combine(Path.GetTempPath(), "git_graph_error.txt"), 
                        $"Error getting Git data: {ex.Message}\n{ex.StackTrace}");
                }

                if (commits.Count == 0)
                {
                    commits.Add(new Dictionary<string, string>
                    {
                        ["hash"] = "1",
                        ["message"] = "Initial commit",
                        ["author"] = "Gaurav",
                        ["email"] = "gaurav.gokhale@mendix.com",
                        ["date"] = "2025-06-09 23:46",
                        ["parent"] = "",
                        ["branch"] = "main"
                    });
                    commits.Add(new Dictionary<string, string>
                    {
                        ["hash"] = "2",
                        ["message"] = "Second commit",
                        ["author"] = "Gaurav",
                        ["email"] = "gaurav.gokhale@mendix.com",
                        ["date"] = "2025-06-09 23:50",
                        ["parent"] = "1",
                        ["branch"] = "main"
                    });
                }

                string commitsJson = JsonSerializer.Serialize(commits);

                string html = $@"
                              <!DOCTYPE html>
                              <html lang='en'>
                              <head>
                                <meta charset='UTF-8' />
                                <title>Git Graph</title>
                                <script src='https://cdn.jsdelivr.net/npm/@gitgraph/js'></script>
                                <style>
                                  body {{
                                    font-family: 'Segoe UI', sans-serif;
                                    background-color: #f4f4f4;
                                    margin: 0;
                                    padding: 0;
                                  }}
                                  h1 {{
                                    text-align: center;
                                    margin-top: 20px;
                                    font-size: 28px;
                                    color: #1976d2;
                                  }}
                                  .path {{
                                    text-align: center;
                                    font-size: 14px;
                                    color: #555;
                                    margin-bottom: 10px;
                                  }}
                                  #graph-container {{
                                    width: 100%;
                                    height: 100%;
                                    background: #ffffff;
                                    border-radius: 8px;
                                    box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
                                    overflow-x: auto;
                                    padding: 20px 0;
                                  }}
                                  .details-box {{
                                    margin: 20px auto;
                                    width: 90%;
                                    max-width: 800px;
                                    background: #fff;
                                    border: 1px solid #ddd;
                                    padding: 20px;
                                    border-radius: 8px;
                                    font-size: 14px;
                                    line-height: 1.5;
                                    box-shadow: 0 0 8px rgba(0, 0, 0, 0.05);
                                  }}
                                  .details-box strong {{
                                    font-size: 16px;
                                    display: block;
                                    margin-bottom: 10px;
                                    color: #1976d2;
                                  }}
                                  pre {{
                                    font-family: monospace;
                                    background-color: #f1f1f1;
                                    padding: 10px;
                                    border-radius: 4px;
                                    overflow-x: auto;
                                  }}
                                  .gitgraph-commit-message {{
                                    cursor: pointer;
                                  }}
                                  #show-all-btn {{
                                    display: block;
                                    margin: 10px auto;
                                    padding: 8px 16px;
                                    background-color: #1976d2;
                                    color: white;
                                    border: none;
                                    border-radius: 4px;
                                    font-size: 14px;
                                    cursor: pointer;
                                  }}
                                  #all-commits-box {{
                                    display: none;
                                    margin: 20px auto;
                                    width: 90%;
                                    max-width: 800px;
                                    background: #fff;
                                    border: 1px solid #ccc;
                                    padding: 15px;
                                    border-radius: 8px;
                                    font-size: 13px;
                                    box-shadow: 0 0 6px rgba(0, 0, 0, 0.1);
                                  }}
                                  #all-commits-box ul {{
                                    list-style: none;
                                    padding-left: 0;
                                  }}
                                  #all-commits-box li {{
                                    margin-bottom: 10px;
                                    padding-bottom: 10px;
                                    border-bottom: 1px dashed #ddd;
                                  }}
                                </style>
                              </head>
                              <body>
                                <h1>Git Graph: {projectName}</h1>
                                <div class='path'><em>{projectPath}</em></div>
                                <div id='graph-container'></div>

                                <div class='details-box' id='commit-details'>
                                  <strong>Click a commit dot to view details</strong>
                                  <pre id='commit-details-content'>No commit selected.</pre>
                                </div>

                                <button id='show-all-btn'>Show All Commits</button>
                                <div id='all-commits-box'>
                                  <strong>All Commits</strong>
                                  <ul id='all-commits-list'></ul>
                                </div>

                                <script>
                                  const commitData = {commitsJson};

                                  window.onload = function () {{
                                    const graphContainer = document.getElementById('graph-container');
                                    const gitgraph = GitgraphJS.createGitgraph(graphContainer, {{
                                      template: GitgraphJS.templateExtend('metro', {{
                                        colors: ['#1565c0', '#fbc02d', '#8bc34a', '#e91e63'],
                                        commit: {{
                                          spacing: 70,
                                          message: {{
                                            displayHash: true,
                                            displayAuthor: true,
                                            font: 'normal 18px Segoe UI',
                                          }},
                                        }},
                                        branch: {{
                                          lineWidth: 10,
                                          spacing: 70,
                                          labelFont: 'normal 14px Segoe UI',
                                        }},
                                      }}),
                                    }});

                                    const branches = {{}};

                                    commitData.forEach((commit) => {{
                                      const branchName = (commit.branch || 'main').split('~')[0];

                                      if (!branches[branchName]) {{
                                        branches[branchName] = gitgraph.branch(branchName);
                                      }}

                                      branches[branchName].commit({{
                                        subject: commit.message,
                                        author: `${{commit.author}} <${{commit.email}}>`,
                                        hash: commit.hash,
                                        parents: commit.parent ? commit.parent.split(' ') : [],
                                        style: {{
                                          dot: {{
                                            color: null,
                                          }},
                                        }},
                                        onMessageClick: () => {{
                                          const content = `
                              Hash: ${{commit.hash}}
                              Message: ${{commit.message}}
                              Author: ${{commit.author}}
                              Email: ${{commit.email}}
                              Date: ${{commit.date}}
                              Branch: ${{commit.branch}}
                              Parents: ${{commit.parent || 'None'}}
                              `;
                                          document.getElementById('commit-details-content').innerText = content;
                                        }}
                                      }});
                                    }});

                                    document.getElementById('show-all-btn').addEventListener('click', function () {{
                                  const listBox = document.getElementById('all-commits-box');
                                  const list = document.getElementById('all-commits-list');
                                  list.innerHTML = '';
                                  commitData.forEach((commit) => {{
                                    const item = document.createElement('li');
                                    item.textContent = `(${{commit.hash}}) ${{commit.message}} by ${{commit.author}} on ${{commit.date}}`;
                                    list.appendChild(item);
                                  }});
                                  listBox.style.display = listBox.style.display === 'none' ? 'block' : 'none';
                                }});
                                  }};
                                </script>
                              </body>
                              </html>
                              ";

                

                
                // Save to temp file and load with proper URI format
                string tempPath = Path.Combine(Path.GetTempPath(), "git_graph_in_mendix.html");
                File.WriteAllText(tempPath, html);
                
                // Format URI correctly for different platforms
                string uriPrefix = "file://";
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    // Windows needs an extra slash
                    uriPrefix = "file:///";
                }
                else if (Environment.OSVersion.Platform == PlatformID.Unix || 
                         Environment.OSVersion.Platform == PlatformID.MacOSX)
                {
                    // macOS/Unix format
                    uriPrefix = "file://";
                }
                
                // Create a properly formatted URI
                Uri fileUri = new Uri(uriPrefix + tempPath.Replace("\\", "/"));
                
                // Log the URI for debugging
                File.WriteAllText(Path.Combine(Path.GetTempPath(), "git_graph_uri.txt"), 
                    $"Loading URI: {fileUri}\nTemp path: {tempPath}");
                
                // Load the HTML file into the WebView
                webView.Address = fileUri;
            }
            catch (Exception ex)
            {
                // Log the error
                File.WriteAllText(Path.Combine(Path.GetTempPath(), "git_graph_error.txt"), 
                    $"Error: {ex.Message}\nStack trace: {ex.StackTrace}");
                
                // Create a simple error page
                string errorHtml = $"<html><body style='background-color: #ff0000; color: white; padding: 20px; font-family: Arial;'><h1>Error</h1><p>{ex.Message}</p><p>{ex.StackTrace}</p></body></html>";
                string tempPath = Path.Combine(Path.GetTempPath(), "git_graph_error.html");
                File.WriteAllText(tempPath, errorHtml);
                
                // Format URI correctly
                string uriPrefix = Environment.OSVersion.Platform == PlatformID.Win32NT ? "file:///" : "file://";
                Uri errorUri = new Uri(uriPrefix + tempPath.Replace("\\", "/"));
                
                webView.Address = errorUri;
            }
        }
    }
}
