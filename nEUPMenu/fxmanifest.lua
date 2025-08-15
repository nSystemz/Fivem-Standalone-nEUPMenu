fx_version 'cerulean'
game 'gta5'

author 'Nemesus | Nemesus.de'
description 'nEUPMenu - Easy EUP menu'
version '1.0.0'

server_scripts {
    '@mysql-async/lib/MySQL.cs',
    'Server.net.dll'
}

client_scripts {
    'Client.net.dll'
}

ui_page 'html/index.html'

files {
    'html/index.html',
    'html/assets/*',
    'html/assets/**/*'
}