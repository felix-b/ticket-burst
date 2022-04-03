if [[ $# -ge 2 ]]; then
    export CDK_DEPLOYTO_ACCOUNT=$1
    export CDK_DEPLOYTO_REGION=$2
    npx cdk deploy "$@"
    exit $?
else
    echo 1>&2 "call: ./deployto.sh <account> <region> [args...]" 
    echo 1>&2 "  runs cdk deploy to specified environment" 
    echo 1>&2 "  additional args will be passed through to cdk deploy" 
    exit 1
fi
