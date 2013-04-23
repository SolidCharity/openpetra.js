#!/bin/sh
#
# chkconfig: 345 96 24
# description: Starts and stops the openpetraorg server running with Mono
#

# Find the name of the script
NAME=`basename $0`
if [ ${NAME:0:1} = "S" -o ${NAME:0:1} = "K" ]
then
        NAME=${NAME:3}
fi

if [ ! -d $OpenPetraOrgPath ]
then
  export mono_path=/opt/mono-openpetra
  export FASTCGI_MONO_SERVER=$mono_path/bin/fastcgi-mono-server4
  export mono=$mono_path/bin/mono
  export OpenPetraOrgPath=/usr/local/openpetraorg
  export CustomerName=DefaultTOREPLACE
  export OPENPETRA_RDBMSType=postgresql
  export OPENPETRA_DBPWD=TOBESETBYINSTALLER
  export OPENPETRA_DBHOST=localhost
  export OPENPETRA_DBPORT=5432
  export OPENPETRA_DBUSER=petraserver
  export OPENPETRA_DBNAME=openpetra
  export OPENPETRA_PORT=9000
  export backupfile=$OpenPetraOrgPath/backup30/backup-`date +%Y%m%d`.sql.gz
fi

# Override defaults from /etc/sysconfig/openpetra if file is present
[ -f /etc/sysconfig/openpetra/${NAME} ] && . /etc/sysconfig/openpetra/${NAME}

if [ "$2" != "" ]
then
  backupfile=$2
  useremail=$2
  ymlgzfile=$2
fi

. /lib/lsb/init-functions

log_daemon_msg() { logger "$@"; echo "$@"; }
log_end_msg() { [ $1 -eq 0 ] && RES=OK; logger ${RES:=FAIL}; }

# start the openpetraorg server
start() {
    log_daemon_msg "Starting OpenPetra.org server for $CustomerName"

    su $userName -c "PATH=$mono_path/bin:$PATH $FASTCGI_MONO_SERVER /socket=tcp:127.0.0.1:$OPENPETRA_PORT /applications=/:/var/www/html /appconfigfile=$OpenPetraOrgPath/etc30/PetraServerConsole.config&"

    status=0
    log_end_msg $status
}

# stop the openpetraorg server
stop() {
    log_daemon_msg "Stopping OpenPetra.org server for $CustomerName"
    cd $OpenPetraOrgPath/bin30
    
    su $userName -c "$mono --runtime=v4.0 --server PetraServerAdminConsole.exe -C:$OpenPetraOrgPath/etc30/PetraServerAdminConsole.config -Command:Stop"
    
    status=0
    log_end_msg $status
}

# load a new database from a yml.gz file. this will overwrite the current database!
loadYmlGz() {
    cd $OpenPetraOrgPath/bin30
    parameters="-Server.Port:$OPENPETRA_PORT -Server.ChannelEncryption.PublicKeyfile:$OPENPETRA_LocationPublicKeyFile"
    su $userName -c "$mono --runtime=v4.0 --server PetraServerAdminConsole.exe -C:$OpenPetraOrgPath/etc30/PetraServerAdminConsole.config $parameters -Command:LoadYmlGz -YmlGzFile:$ymlgzfile"
    status=0
    log_end_msg $status
}

# display a menu to check for logged in users etc
menu() {
    cd $OpenPetraOrgPath/bin30
    parameters="-Server.Port:$OPENPETRA_PORT -Server.ChannelEncryption.PublicKeyfile:$OPENPETRA_LocationPublicKeyFile"
    su $userName -c "$mono --runtime=v4.0 --server PetraServerAdminConsole.exe -C:$OpenPetraOrgPath/etc30/PetraServerAdminConsole.config $parameters"
}

# backup the postgresql database
backup() {
    echo `date` "Writing to " $backupfile
    # loading of this dump will show errors about existing data tables etc.
    # could have 2 calls: --data-only and --schema-only.
    su $userName -c "pg_dump -h $OPENPETRA_DBHOST -p $OPENPETRA_DBPORT -U $OPENPETRA_DBUSER $OPENPETRA_DBNAME | gzip > $backupfile"
    echo `date` "Finished!"
}

# restore the postgresql database
restore() {
    echo "This will overwrite your database!!!"
    echo "Please enter 'yes' if that is ok:"
    read response
    if [ "$response" != 'yes' ]
    then
        echo "Cancelled the restore"
        exit
    fi
    echo `date` "Start restoring from " $backupfile
    echo "dropping tables and sequences..."

    #echo $OPENPETRA_DBHOST:$OPENPETRA_DBPORT:$OPENPETRA_DBNAME:$OPENPETRA_DBUSER:$OPENPETRA_DBPWD > ~/.pgpass

    delCommand="SELECT 'DROP TABLE ' || n.nspname || '.' || c.relname || ' CASCADE;' FROM pg_catalog.pg_class AS c LEFT JOIN pg_catalog.pg_namespace AS n ON n.oid = c.relnamespace WHERE relkind = 'r' AND n.nspname NOT IN ('pg_catalog', 'pg_toast') AND pg_catalog.pg_table_is_visible(c.oid)" 
    su $userName -c "psql -t -h $OPENPETRA_DBHOST -p $OPENPETRA_DBPORT -U $OPENPETRA_DBUSER $OPENPETRA_DBNAME -c \"$delCommand\" > /tmp/deleteAllTables.sql"

    delCommand="SELECT 'DROP SEQUENCE ' || n.nspname || '.' || c.relname || ';' FROM pg_catalog.pg_class AS c LEFT JOIN pg_catalog.pg_namespace AS n ON n.oid = c.relnamespace WHERE relkind = 'S' AND n.nspname NOT IN ('pg_catalog', 'pg_toast') AND pg_catalog.pg_table_is_visible(c.oid)" 
    su $userName -c "psql -t -h $OPENPETRA_DBHOST -p $OPENPETRA_DBPORT -U $OPENPETRA_DBUSER $OPENPETRA_DBNAME -c \"$delCommand\" > /tmp/deleteAllSequences.sql"
    su $userName -c "psql -h $OPENPETRA_DBHOST -p $OPENPETRA_DBPORT -U $OPENPETRA_DBUSER $OPENPETRA_DBNAME -q -f /tmp/deleteAllTables.sql"
    su $userName -c "psql -h $OPENPETRA_DBHOST -p $OPENPETRA_DBPORT -U $OPENPETRA_DBUSER $OPENPETRA_DBNAME -q -f /tmp/deleteAllSequences.sql"

    rm /tmp/deleteAllTables.sql
    rm /tmp/deleteAllSequences.sql

    export PGOPTIONS='--client-min-messages=warning'

    echo "creating tables..."
    su $userName -c "psql -h $OPENPETRA_DBHOST -p $OPENPETRA_DBPORT -U $OPENPETRA_DBUSER $OPENPETRA_DBNAME -q -f $OpenPetraOrgPath/db30/createtables-PostgreSQL.sql"

    echo "loading data..."
    echo $backupfile|grep -qE '\.gz$'
    if [ $? -eq 0 ]
    then
        su $userName -c "cat $backupfile | gunzip | psql -h $OPENPETRA_DBHOST -p $OPENPETRA_DBPORT -U $OPENPETRA_DBUSER $OPENPETRA_DBNAME -q > $OpenPetraOrgPath/log30/pgload.log"
    else
        su $userName -c "psql -h $OPENPETRA_DBHOST -p $OPENPETRA_DBPORT -U $OPENPETRA_DBUSER $OPENPETRA_DBNAME -q -f $backupfile > $OpenPetraOrgPath/log30/pgload.log"
    fi

    echo "enabling indexes and constraints..."
    su $userName -c "psql -h $OPENPETRA_DBHOST -p $OPENPETRA_DBPORT -U $OPENPETRA_DBUSER $OPENPETRA_DBNAME -q -f $OpenPetraOrgPath/db30/createconstraints-PostgreSQL.sql"

    echo `date` "Finished!"
}

createdb() {
    echo "creating database..."
    su postgres -c "psql -q -p $OPENPETRA_DBPORT -c \"CREATE USER \\\"$OPENPETRA_DBUSER\\\" PASSWORD '$OPENPETRA_DBPWD'\""
    su postgres -c "createdb -p $OPENPETRA_DBPORT -T template0 -O $OPENPETRA_DBUSER $OPENPETRA_DBNAME"
}

init() {
    export backupfile=$OpenPetraOrgPath/db30/demodata-PostgreSQL.sql
    restore
}

case "$1" in
    start)
        start
        ;;
    stop)
        stop
        ;;
    restart)
        stop
        start
        ;;
    backup)
        backup
        ;;
    restore)
        restore
        ;;
    init)
        init
        ;;
    loadYmlGz)
        loadYmlGz
        ;;
    menu)
        menu
        ;;
    *)
        echo "Usage: $0 {start|stop|restart|menu|backup|restore|createdb|init}"
        exit 1
        ;;
esac

exit 0
