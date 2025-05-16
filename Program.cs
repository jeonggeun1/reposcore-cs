using Cocona;
using System;
using System.Collections.Generic;
using Octokit;

CoconaApp.Run((
    [Argument] string[] repos,
    [Option('v', Description = "자세한 로그 출력을 활성화합니다.")] bool verbose,
    [Option('o', Description = "출력 디렉토리 경로를 지정합니다.")] string? output,
    [Option('f', Description = "출력 형식을 지정합니다. (예: json,csv)")] string? format,
    [Option('t', Description = "GitHub Personal Access Token 입력")] string? token
) =>
{
    if (repos.Length != 2)
    {
        Console.WriteLine("! repository 인자는 'owner repo' 순서로 2개가 필요합니다.");
        Environment.Exit(1);
        return;
    }

    string owner = repos[0];
    string repo = repos[1];

    // format 옵션을 쉼표로 분리하여 리스트로 변환
    var formats = string.IsNullOrWhiteSpace(format)
        ? new List<string> { "json" }
        : new List<string>(format.Split(',', StringSplitOptions.RemoveEmptyEntries));

    // 유효한 형식 리스트 정의
    var validFormats = new HashSet<string> { "json", "csv" };

    // 지원하지 않는 형식이 포함되었는지 확인
    foreach (var f in formats)
    {
        if (!validFormats.Contains(f.Trim().ToLower()))
        {
            Console.WriteLine($"! 지원되지 않는 출력 형식입니다: {f} (지원 형식: json, csv)");
            Environment.Exit(1);
        }
    }

    Console.WriteLine($"Repository: {string.Join("\n ", repos)}");

    if (verbose)
    {
        Console.WriteLine("Verbose mode is enabled.");
    }

    try
    {
        var client = new GitHubClient(new ProductHeaderValue("CoconaApp"));

        if (!string.IsNullOrEmpty(token))
        {
            client.Credentials = new Credentials(token);
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
        // 출력 디렉토리 기본값 처리
        var outputDir = string.IsNullOrWhiteSpace(output) ? "output" : output;

        // token이 null일 수 있으므로 안전하게 처리
        var analyzer = new GitHubAnalyzer(token ?? string.Empty);
        analyzer.Analyze(owner, repo, outputDir, formats); // 변경된 Analyze 호출
    }
    catch (Exception ex)
    {
        Console.WriteLine($"! 오류 발생: {ex.Message}");
        Environment.Exit(1);
    }

    Environment.Exit(0);
});
