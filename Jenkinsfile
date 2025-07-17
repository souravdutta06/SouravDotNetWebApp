pipeline {
    agent any
    
    environment {
        DOCKER_HUB_CREDENTIALS = credentials('docker-hub-credentials')
        DOCKER_IMAGE = 'souravdutta06/SouravDotNetWebApp'
        DOCKER_TAG = "latest"
        DEPLOYMENT_SERVER = '20.3.128.98'
        DEPLOYMENT_USER = 'azureuser'
        DEPLOYMENT_SSH_KEY = credentials('azure-vm-ssh-key')
    }
    
    stages {
        stage('Checkout') {
            steps {
                checkout scm
            }
        }
        
        stage('Build') {
            steps {
                sh 'dotnet build'
            }
        }
        
        stage('Test') {
            steps {
                sh 'dotnet test'
            }
        }
        
        stage('Build Docker Image') {
            steps {
                script {
                    docker.build("${DOCKER_IMAGE}:${DOCKER_TAG}")
                }
            }
        }
        
        stage('Push to Docker Hub') {
            steps {
                script {
                    docker.withRegistry('https://registry.hub.docker.com', 'docker-hub-credentials') {
                        docker.image("${DOCKER_IMAGE}:${DOCKER_TAG}").push()
                    }
                }
            }
        }
        
        stage('Deploy to Azure VM') {
            steps {
                script {
                    sshagent(['azure-vm-ssh-key']) {
                        sh """
                            ssh -o StrictHostKeyChecking=no ${DEPLOYMENT_USER}@${DEPLOYMENT_SERVER} \
                            "docker pull ${DOCKER_IMAGE}:${DOCKER_TAG} && \
                            docker stop dotnetwebapp || true && \
                            docker rm dotnetwebapp || true && \
                            docker run -d -p 80:80 --name dotnetwebapp ${DOCKER_IMAGE}:${DOCKER_TAG}"
                        """
                    }
                }
            }
        }
    }
    
    post {
        always {
            cleanWs()
        }
    }
}
