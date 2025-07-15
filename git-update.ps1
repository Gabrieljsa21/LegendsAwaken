# Mostrar status atual do repositório
git status

# Adiciona todas as mudanças (novos arquivos, modificações e deleções)
git add .

# Pede a mensagem do commit
Write-Host "Digite a mensagem do commit:"
$mensagem = Read-Host

# Verifica se há mudanças staged para commit
git diff --cached --quiet
if ($LASTEXITCODE -eq 0) {
    Write-Host "Nada para commitar."
} else {
    git commit -m "$mensagem"
}

# Descobre a branch atual
$branch = git rev-parse --abbrev-ref HEAD

# Envia o commit para o repositório remoto na branch atual
try {
    git push origin $branch
    Write-Host "Atualização enviada para branch $branch"
} catch {
    Write-Host "Erro ao enviar o push. Verifique sua conexão e configurações do Git."
}
