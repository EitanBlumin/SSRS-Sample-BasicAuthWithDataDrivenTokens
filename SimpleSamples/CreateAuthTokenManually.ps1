param(
[string]$ServerInstance = "localhost",
[string]$Database = "SampleWebApp",
[string]$UserId = "fda1aaaf-3b8c-4a06-a85b-201717f4b71a"
)
Process
{
    $scon = New-Object System.Data.SqlClient.SqlConnection
    $scon.ConnectionString = "Server={0};Database={1};Integrated Security=True" -f $ServerInstance,$Database
        
    $cmd = New-Object System.Data.SqlClient.SqlCommand
    $cmd.Connection = $scon
    $cmd.CommandTimeout = 40

    $cmd.CommandText = "EXEC [dbo].[CreateSSRSAuthToken] @UserId"
    $cmd.Parameters.Add((New-Object System.Data.SqlClient.SqlParameter("@UserId",$UserId))) | Out-Null

    try
    {
        $scon.Open()
            
        $ds=New-Object system.Data.DataSet
        $da=New-Object system.Data.SqlClient.SqlDataAdapter($cmd)
        [void]$da.fill($ds)
        $scon.Close()
        foreach ($r in $ds.Tables[0].Rows)
        {
            Write-Output "=== Generated Token: ==="
            Write-Output "UserId: $($r.UserId)"
            Write-Output "HashToken: $($r.HashToken)"
            Write-Output "ExpireDate: $($r.ExpireDate)"
        }
    }
    catch [Exception]
    {
        Write-Warning $_.Exception.Message
    }
    finally
    {
        $scon.Dispose()
        $cmd.Dispose()
    }
}