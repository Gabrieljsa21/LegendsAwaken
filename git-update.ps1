# Mostrar status atual do reposit�rio
git status

# Adiciona todas as mudan�as (novos arquivos, modifica��es e dele��es)
git add .

# Pede a mensagem do commit
Write-Host "Digite a mensagem do commit:"
$mensagem = Read-Host

# Verifica se h� mudan�as staged para commit
git diff --cached --quiet
if ($LASTEXITCODE -eq 0) {
    Write-Host "Nada para commitar."
} else {
    git commit -m "$mensagem"
}

# Descobre a branch atual
$branch = git rev-parse --abbrev-ref HEAD

# Envia o commit para o reposit�rio remoto na branch atual
try {
    git push origin $branch
    Write-Host "Atualiza��o enviada para branch $branch"
} catch {
    Write-Host "Erro ao enviar o push. Verifique sua conex�o e configura��es do Git."
}
