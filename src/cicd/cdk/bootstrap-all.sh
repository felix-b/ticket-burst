export CDK_NEW_BOOTSTRAP=1

cdk bootstrap aws://989574085263/sa-east-1

cdk bootstrap --trust=989574085263 --cloudformation-execution-policies=arn:aws:iam::aws:policy/AdministratorAccess --verbose


