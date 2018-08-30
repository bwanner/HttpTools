
function ConvertTo-String($secureString) {
    $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($secureString)
    return [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
}

function Utils-MD5($string) {
    $md5 = new-object -TypeName System.Security.Cryptography.MD5CryptoServiceProvider
    $utf8 = new-object -TypeName System.Text.UTF8Encoding
    return ([System.BitConverter]::ToString($md5.ComputeHash($utf8.GetBytes($string))) -replace "-","").ToLower()
}

# Must match HttpTools::HttpServer(Batzill.Server.Core.Utils.GenerateKeyHash)
$rep = 42
$salt = "bae83a2e-13cd-4b85-a670-24ebd1d88e99"

$key = ConvertTo-String (read-host "Enter key" -AsSecureString)
for($i = 0; $i -lt $rep; $i++)
{
    $key = Utils-MD5 "$key#$salt"
}

return $key