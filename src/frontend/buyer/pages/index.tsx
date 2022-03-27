import React, { useEffect } from 'react'
import { Box, Center, Flex, Grid, GridItem, HStack, Spacer, Image, Img, AspectRatio, Stack, Text, Button, Tag, TagLabel, Stat, StatLabel, StatNumber, StatHelpText, StatGroup } from "@chakra-ui/react"

const IndexPage = () => {

    const headerRef = React.useRef<HTMLDivElement>(null);
    /*
    function handleWindowScroll() {
        if (document.body.scrollTop > 5 || document.documentElement.scrollTop > 5) {
            //headerRef.current.style.height = '70px';
            headerRef.current.style.backgroundColor = 'var(--chakra-colors-green-600)';
        } else {
            //headerRef.current.style.height = '100px';
            headerRef.current.style.backgroundColor = 'var(--chakra-colors-blue-800)';
        }
    }
    
    useEffect(() => {
        window.addEventListener('scroll', handleWindowScroll);
        return function cleanup() {
            window.removeEventListener('scroll', handleWindowScroll);
        };
    });
    */

    return (
        <>
            <Flex as="header" 
                ref={headerRef} 
                position="fixed" 
                w="100%" 
                h="100px" 
                justifyContent={'center'}
                backgroundColor="green.600" 
                style={{transition: '0.2s', zIndex: 1000}}
            >
                <Flex direction={'row'} w={1200} h="100%" alignItems={'center'}>
                    <Box>

                    </Box>
                    <Spacer />
                    <Box>
                        <Text color='white' textAlign={'center'} fontSize='5xl' fontWeight={900}>Summer Olympic Games 2024</Text>
                    </Box>
                    <Spacer />
                    <Box>

                    </Box>
                </Flex>
            </Flex>
            <Grid 
                minH="100vh"
                bg="white" 
                templateRows="500px auto"
                templateColumns="auto 1200px auto"
            >
                <GridItem  colSpan={3} bg="blue.800" pt="100px" >
                    <Center>
                        <Box w={1200}  mt='40px'>
                            <Stack w="100%" spacing={0} alignContent='center' color='white' justifyItems={'center'}>
                                <Text textAlign={'center'} fontSize='3xl' fontWeight={'bold'} >21 August 2024</Text>
                                <Text textAlign={'center'} fontSize='5xl' fontWeight={'bold'} >Brazil - Germany</Text>
                                <Text textAlign={'center'} fontSize='2xl' fontWeight={'bold'} >$110 - $220</Text>
                                <Box>
                                    <Center>
                                        <Button mt='20px' mb='20px' colorScheme='red' size='lg' fontWeight={'bold'} fontSize='2xl' minW='300px' h='60px'>
                                            Grab Your Seats
                                            <Tag size={'sm'} borderRadius='full' variant='solid' colorScheme='purple' ml='10px'>
                                                <TagLabel fontSize={'sm'}>92 left</TagLabel>
                                            </Tag>                                            
                                        </Button>
                                    </Center>
                                </Box>
                                <Text textAlign={'center'} fontSize='4xl' fontWeight={'bold'} >Football 1/4 Final</Text>
                                <Text textAlign={'center'} fontSize='4xl' >Arena BRB Mane Garrincha</Text>
                            </Stack>
                        </Box>
                    </Center>
                </GridItem>
                <GridItem  h="full"></GridItem>
                <GridItem  h="full" minH={2000}>

                    <Flex gap='10px' mt='40px'>
                        <StatGroup w='370px' border={'1px solid lightgray'} p={4} borderRadius={'xl'}>
                            <Stat>
                                <StatLabel>Collected Fees</StatLabel>
                                <StatNumber>£0.00</StatNumber>
                                <StatHelpText>Feb 12 - Feb 28</StatHelpText>
                            </Stat>
                        </StatGroup>
                        <Spacer/>
                        <StatGroup w='370px' border={'1px solid lightgray'} p={4} borderRadius={'xl'}>
                            <Stat>
                                <StatLabel>Collected Fees</StatLabel>
                                <StatNumber>£0.00</StatNumber>
                                <StatHelpText>Feb 12 - Feb 28</StatHelpText>
                            </Stat>
                        </StatGroup>
                        <Spacer/>
                        <StatGroup w='370px' border={'1px solid lightgray'} p={4} borderRadius={'xl'}>
                            <Stat>
                                <StatLabel>Collected Fees</StatLabel>
                                <StatNumber>£0.00</StatNumber>
                                <StatHelpText>Feb 12 - Feb 28</StatHelpText>
                            </Stat>
                        </StatGroup>

                    </Flex>
                    
                </GridItem>
                <GridItem  h="full"></GridItem>
            </Grid>  
        </>
    )
}

export default IndexPage
