using Cocona;
using System;
using System.Collections.Generic;
using Octokit;

CoconaApp.Run((
    [Argument(Description = "저장소 소유자와 이름 (예: openai chatgpt)")] string[] repos,
    [Option('v', Description = "자세한 로그 출력을 활성화합니다.")] bool verbose,
    [Option('o', Description = "출력 디렉토리 경로를 지정합니다.")] string? output,
    [Option("format", new[] { 'f' }, Description = "출력 형식을 지정합니다. (예: json csv)")] string[] formats,
    [Option('t', Description = "GitHub Personal Access Token 입력")] string? token
) =>
{
    void Log(string message)
    {
        if (verbose)
        {
            Console.WriteLine("[VERBOSE] " + message);
        }
    }

    if (repos.Length != 2)
    {
        Console.WriteLine("! repository 인자는 'owner repo' 순서로 2개가 필요합니다.");
        Environment.Exit(1);
        return;
    }

    string owner = repos[0];
    string repo = repos[1];

    Console.WriteLine($"Repository: {owner}/{repo}");
    Log("Verbose mode is enabled.");

    try
    {
        var client = new GitHubClient(new ProductHeaderValue("CoconaApp"));

        if (!string.IsNullOrEmpty(token))
        {
            client.Credentials = new Credentials(token);
        }
        else
        {
            Log("GitHub 토큰이 제공되지 않았습니다. Rate limit에 주의하세요.");
        }

        var repository = client.Repository.Get(owner, repo).GetAwaiter().GetResult();

        Console.WriteLine($"[INFO] Repository Name: {repository.Name}");
        Console.WriteLine($"[INFO] Full Name: {repository.FullName}");
        Console.WriteLine($"[INFO] Description: {repository.Description}");
        Console.WriteLine($"[INFO] Stars: {repository.StargazersCount}");
        Console.WriteLine($"[INFO] Forks: {repository.ForksCount}");
        Console.WriteLine($"[INFO] Open Issues: {repository.OpenIssuesCount}");
        Console.WriteLine($"[INFO] Language: {repository.Language}");
        Console.WriteLine($"[INFO] URL: {repository.HtmlUrl}");
    }
    catch (Exception e)
    {
        Console.WriteLine($"! 오류 발생: {e.Message}");
        Environment.Exit(1);
    }

    try
    {
        var supportedFormats = new[] { "json", "csv" };
        var formatsToUse = (formats.Length == 0) ? new[] { "json" } : formats;

        foreach (var f in formatsToUse)
        {
            if (!supportedFormats.Contains(f.ToLower()))
            {
                Console.WriteLine($"❗ 지원되지 않는 출력 형식입니다: {f} (지원 형식: json, csv)");
                Environment.Exit(1);
            }
        }

        var outputDir = string.IsNullOrWhiteSpace(output) ? "output" : output;

        var analyzer = new GitHubAnalyzer(token!);
        analyzer.Analyze(owner, repo, outputDir, new List<string>(formatsToUse));
    }
    catch (Exception ex)
    {
        Console.WriteLine($"! 오류 발생: {ex.Message}");
        Environment.Exit(1);
    }

    Environment.Exit(0);
});
